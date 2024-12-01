using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;
using System.Security.Permissions;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
using System.Net.Http;
#endregion

namespace KeyLogger.Macros
{
	/// <summary>
	/// Тип макроса
	/// </summary>
	public enum MacroType : byte
	{
		KEYBOARD_EVENT,
		MOUSE_EVENT,
		WAITTIME,
		WAIT_TILL_KEY,
		LOOP,
		END_OF_LOOP
	}
	/// <summary>
	/// Базовый класс для всех элементов макроса
	/// </summary>
	public abstract class MacroElement
	{
		protected Func<MacroPlayer, Task> action;
		public Func<MacroPlayer, Task> Execute
		{
			get => action;
		}

		protected MacroType macroType;
		public MacroType MacroType
		{
			get => macroType;
		}

		public MacroElement()
		{
			
		}

		protected Func<string> stringRepresentationFunc;
		public string StringRepresentation
		{
			get => stringRepresentationFunc();
		}

		public override string ToString() => StringRepresentation;

		public static MacroElement CreateFromJson(JsonElement macroJsonData)
		{
			byte macroType = macroJsonData.GetProperty("type").GetByte();

			switch (macroType)
			{
				case (byte)MacroType.KEYBOARD_EVENT:
					return new KeyboardEventMacroElement(macroJsonData);
				case (byte)MacroType.MOUSE_EVENT:
					return new MouseEventMacroElement(macroJsonData);
				case (byte)MacroType.WAITTIME:
					return new WaitTimeMacroElement(macroJsonData);
				case (byte)MacroType.WAIT_TILL_KEY:
                    throw new InvalidMacroTokenException("Макрос такого типа еще не поддерживается: " + macroType);
                case (byte)MacroType.LOOP:
					return new LoopMacroElement(macroJsonData);
				case (byte)MacroType.END_OF_LOOP:
					return new EndOfLoopMacroElement(macroJsonData);
				default:
					throw new InvalidMacroTokenException("Неизвестный тип макроса: " + macroType);
			}
		}

		public abstract void WriteToJson(ref Utf8JsonWriter writer);
	}

	public enum KeyEventType : byte
	{
		VIRTUAL_KEY,
		EXTENDED_KEY,
		SCANCODE,
		UNICODE
	}
	public struct KeyInputData
	{
		public ushort KeyCode;
		public KeyEventType KeyEventType;
	}
	public class KeyboardEventMacroElement : MacroElement
	{
		private KeyInputData[] _keys;

		private INPUT[][] _inputs = new INPUT[2][];

		public ushort[] KeyCodes 
		{
			get => _keys.Select((key) => key.KeyCode).ToArray();
			set
			{
				for (int i = 0; i < _keys.Length && i < value.Length; i++)
				{
					_keys[i].KeyCode = value[i];
				}
			}
		}
		public KeyEventType[] KeyTypes
		{
			get => _keys.Select((key) => key.KeyEventType).ToArray();
			set
			{
				for (int i = 0; i < _keys.Length && i < value.Length; i++)
				{
					_keys[i].KeyEventType = value[i];
				}
			}
		}

		private bool _doKeyDown;
		public bool DoKeyDown
		{
			get => _doKeyDown;
		}
		private bool _doKeyUp;
		public bool DoKeyUp
		{
			get => _doKeyUp;
		}

		public string InputFlags
		{
			get => ((_doKeyDown) ? "D" : "") + ((_doKeyUp) ? "U" : "");
		}

		public KeyboardEventMacroElement(ushort[] keyCodes, KeyEventType[] keyTypes, bool doKeyDown, bool doKeyUp)
		{
			KeyInputData[] keys = new KeyInputData[keyCodes.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = new KeyInputData() { KeyCode = keyCodes[i], KeyEventType = keyTypes[i] };
            }

			Initialize(keys, doKeyDown, doKeyUp);
		}
        public KeyboardEventMacroElement(KeyInputData[] keys, bool doKeyDown, bool doKeyUp) => Initialize(keys, doKeyDown, doKeyUp);

        private void Initialize(KeyInputData[] keyInputs, bool doKeyDown, bool doKeyUp)
		{
            macroType = MacroType.KEYBOARD_EVENT;
			_keys = keyInputs;
			_doKeyDown = doKeyDown;
			_doKeyUp = doKeyUp;

			stringRepresentationFunc = () =>
			{
				string representation = string.Empty;
				if (_doKeyUp && !_doKeyDown)
					representation = "Отпустить";
				else if (!_doKeyUp && _doKeyDown)
					representation = "Удерживать";
				else
					representation = "Нажать";

				representation += ": ";
				for (int i = 0; i < _keys.Length; i++)
				{
					switch (_keys[i].KeyEventType)
					{
						case KeyEventType.VIRTUAL_KEY:
                            representation += "VK " + Enum.GetName(typeof(VK), _keys[i].KeyCode).Substring(2);
                            break;
						case KeyEventType.EXTENDED_KEY:
                            representation += "EK " + Enum.GetName(typeof(VK), _keys[i].KeyCode).Substring(2);
                            break;
						case KeyEventType.SCANCODE:
                            representation += "SC " + Enum.GetName(typeof(VK), _keys[i].KeyCode).Substring(2);
                            break;
						case KeyEventType.UNICODE:
                            representation += "UC " + Enum.GetName(typeof(VK), _keys[i].KeyCode).Substring(2);
                            break;
						default:
							break;
					}
					representation += " + ";
				}
				representation = representation.Remove(representation.Length - 3);
				return representation;
			};
            
            action = async (player) =>
			{
                KEYBDINPUT[] keys = new KEYBDINPUT[_keys.Length];
                for (int i = 0; i < _keys.Length; i++)
                {
                    uint flags = 0;
                    ushort vkCode = 0;
                    ushort scanCode = 0;

                    if (_keys != null)
                    {
                        if (i < _keys.Length)
                            switch (_keys[i].KeyEventType)
                            {
                                case KeyEventType.VIRTUAL_KEY:
                                    vkCode = _keys[i].KeyCode;
                                    break;
                                case KeyEventType.EXTENDED_KEY:
                                    flags = 0x0008;
                                    scanCode = (ushort)((0xE0 << 2) | (0x00FF & _keys[i].KeyCode));
                                    break;
                                case KeyEventType.SCANCODE:
                                    flags = 0x0008;
                                    scanCode = _keys[i].KeyCode;
                                    break;
                                case KeyEventType.UNICODE:
                                    flags = 0x0004;
                                    scanCode = _keys[i].KeyCode;
                                    break;
                                default:
                                    break;
                            }
                    }

                    keys[i] = new KEYBDINPUT()
                    {
                        ScanCode = scanCode,
                        VkCode = vkCode,
                        ExtraInfo = IntPtr.Zero,
                        Flags = flags,
                        Time = 0
                    };
                }
                if (_doKeyDown)
				{
					_inputs[0] = new INPUT[_keys.Length];

					for (int i = 0; i < _keys.Length; i++)
					{
						_inputs[0][i] = new INPUT()
						{
							Type = (uint)INPUTTYPE.KEYBOARD,
							Data = new MOUSEKEYBDHARDWAREINPUT()
							{
								Keyboard = new KEYBDINPUT()
								{
									VkCode = keys[i].VkCode,
									ExtraInfo = keys[i].ExtraInfo,
									Flags = keys[i].Flags & ~(uint)KEYEVENTF.KEYUP,
									ScanCode = keys[i].ScanCode,
									Time = keys[i].Time
								}
							}
						};
					}

					SendInput((uint)_inputs[0].Length, _inputs[0], Marshal.SizeOf(typeof(INPUT)));
				}

				if (_doKeyUp)
				{
					if (_doKeyDown)
						await Task.Delay(32);

					_inputs[1] = new INPUT[_keys.Length];
					for (int i = 0; i < _keys.Length; i++)
					{
						_inputs[1][i] = new INPUT()
						{
							Type = (uint)INPUTTYPE.KEYBOARD,
							Data = new MOUSEKEYBDHARDWAREINPUT()
							{
								Keyboard = new KEYBDINPUT()
								{
									VkCode = keys[i].VkCode,
									ExtraInfo = keys[i].ExtraInfo,
									Flags = keys[i].Flags | (uint)KEYEVENTF.KEYUP,
									ScanCode = keys[i].ScanCode,
									Time = keys[i].Time
								}
							}
						};

					}
					SendInput((uint)_inputs[1].Length, _inputs[1], Marshal.SizeOf(typeof(INPUT)));
				}
			};
		}

		public KeyboardEventMacroElement(JsonElement macroJsonData)
		{
			try
			{
				/*
				 JSON:
					{
						flags: "DU",
						keys: [
							{	keycode: 10, keyeventtype: 2 },
							...
						]
					}
				 */
				bool doKeyDown = false, doKeyUp = false;
				string inputFlags = macroJsonData.GetProperty("flags").GetString();
				JsonElement.ArrayEnumerator keysJsonData = macroJsonData.GetProperty("keys").EnumerateArray();
				ushort[] keyCodes = keysJsonData.Select((el) => el.GetProperty("keyCode").GetUInt16()).ToArray();
				KeyEventType[] keyTypes = (keysJsonData.Select((el) => (KeyEventType)el.GetProperty("keyEventType").GetByte()).ToArray());
                if (inputFlags == "DU")
				{
					doKeyDown = true;
					doKeyUp = true;
				}
				else if (inputFlags == "D")
				{
					doKeyDown = true;
				}
                else if (inputFlags == "U")
                {
                    doKeyUp = true;
                }
				else
				{
					throw new InvalidMacroTokenException("Некорректное значение атрибута Flags (ожидалось одно из: DU, U, D");
				}

                KeyInputData[] keys = new KeyInputData[keyCodes.Length];
                for (int i = 0; i < keyCodes.Length; i++)
                {
                    keys[i] = new KeyInputData()
                    {
                        KeyCode = keyCodes[i],
                        KeyEventType = keyTypes[i],
                    };
                }

                Initialize(keys, doKeyDown, doKeyUp);
            }
			catch (Exception ex)
			{
				throw new InvalidMacroTokenException("Ошибка загрузки файла макроса:\n" + ex.Message);
			}
		}
        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
			writer.WriteStartObject();
			writer.WriteNumber("type", (byte)macroType);
			writer.WriteString("flags", InputFlags);
			writer.WritePropertyName("keys");
			writer.WriteStartArray();
			foreach (KeyInputData key in _keys)
			{
				writer.WriteStartObject();
				writer.WriteNumber("keyCode", key.KeyCode);
				writer.WriteNumber("keyEventType", (byte)key.KeyEventType);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

	public enum MouseButton : uint
	{
		NOTHING = 0x0000,
		LMB = 0x0002,
		RMB = 0x0008,
		MMB = 0x0020,
		XMB = 0x0080,
		WHEEL = 0x0800,
		HWHEEL = 0x2000,
	}
	public class MouseEventMacroElement : MacroElement
	{
		private INPUT[][] _inputs = new INPUT[2][];

		private MouseButton _buttonPressed;

		private bool _doMove;
		public bool DoMove
		{
			get => _doMove;
			set => _doMove = value;
        }
		private bool _isAbsolutePositioning;
		public bool IsAbsolutePositioning
		{
            get => _isAbsolutePositioning;
            set => _isAbsolutePositioning = value;
        }

		private int _x;
		public int X
		{
			get => _x;
			set => _x = value;
		}
		public int NormalizedX
		{
			get => (int)((double)X / (double)ScreenX * (double)ushort.MaxValue);
		}
		private int _y;
		public int Y
		{
			get => _y;
			set => _y = value;
		}
		public int NormalizedY
		{
			get => (int)((double)Y / (double)ScreenY * (double)ushort.MaxValue);
		}

		private int _xmbButtonNumber;
		public int XmbButtonNumber
		{
			get => _xmbButtonNumber;
			set => _xmbButtonNumber = value;
		}
		private int _mouseWheelMove;
		public int MouseWheelMove
		{
			get => _mouseWheelMove;
			set => _mouseWheelMove = value;
		}

        private bool _doKeyDown;
		public bool DoKeyDown
		{
			get => _doKeyDown;
		}
		private bool _doKeyUp;
		public bool DoKeyUp
		{
			get => _doKeyUp;
		}

		public string InputFlags
		{
			get => ((_doKeyDown) ? "D" : "") + ((_doKeyUp) ? "U" : "");
		}

		public MouseEventMacroElement(bool doKeyDown, bool doKeyUp, MouseButton buttonPressed, bool doMove, bool isAbsolutePositioning, int x, int y, int xmbButtonNumber = 0, int mouseWheelMove = 0) => Initialize( doKeyDown, doKeyUp, buttonPressed, doMove, isAbsolutePositioning, x, y, xmbButtonNumber, mouseWheelMove);
		public MouseEventMacroElement(string flags, MouseButton buttonPressed, bool doMove, bool isAbsolutePositioning, int x, int y, int xmbButtonNumber = 0, int mouseWheelMove = 0)
		{
			bool doKeyDown = false, doKeyUp = false;
			switch (flags)
			{
				case "":
					break;
				case "D":
					doKeyDown = true;
					break;
				case "U":
					doKeyUp = true;
					break;
				case "DU":
					doKeyDown = true;
					doKeyUp = true;
					break;
				default:
					throw new ArgumentException("Аргумент flags должен быть одним из следующих: U, D, DU, *пустая строка*");
			}
            Initialize(doKeyDown, doKeyUp, buttonPressed, doMove, isAbsolutePositioning, x, y, xmbButtonNumber, mouseWheelMove);
        }

		private void Initialize(bool doKeyDown, bool doKeyUp, MouseButton buttonPressed, bool doMove, bool isAbsolutePositioning, int x, int y, int xmbButtonNumber = 0, int mouseWheelMove = 0)
		{
            macroType = MacroType.MOUSE_EVENT;
			_inputs[0] = new INPUT[1];
			_inputs[1] = new INPUT[1];
			_buttonPressed = buttonPressed;
			_doKeyDown = doKeyDown;
			_doKeyUp = doKeyUp;
			_doMove = doMove;
			_isAbsolutePositioning = isAbsolutePositioning;
			_x = x;
			_y = y;
			_xmbButtonNumber = xmbButtonNumber;
			_mouseWheelMove = mouseWheelMove;


			stringRepresentationFunc = () =>
			{
				string representation = string.Empty;
				if (_doKeyUp && !_doKeyDown)
					representation = "Отпустить";
				else if (!_doKeyUp && _doKeyDown)
					representation = "Удерживать";
				else
					representation = "Нажать";

				representation += ": ";
				switch (_buttonPressed)
				{
					case MouseButton.NOTHING:
						representation = "Направить курсор";
						break;
					case MouseButton.LMB:
						representation += "ЛКМ";
						break;
					case MouseButton.RMB:
						representation += "ПКМ";
						break;
					case MouseButton.MMB:
						representation += "СКМ";
						break;
					case MouseButton.XMB:
						representation += $"Кнопка {(_xmbButtonNumber == 0 ? "4" : "5")}";
						break;
					case MouseButton.WHEEL:
						representation = $"Колесо мыши {(_mouseWheelMove > 0 ? "Вверх" : "Вниз")}";
						break;
					case MouseButton.HWHEEL:
						representation = $"Колесо мыши {(_mouseWheelMove > 0 ? "Вправо" : "Влево")}";
						break;
					default:
						break;
				}
				if (_doMove && _buttonPressed != MouseButton.NOTHING)
				{
					representation += $"; X = {_x}, Y = {_y} ({(isAbsolutePositioning ? "абсолютные" : "относительные")})";
				}
				return representation;
			};
			action = async (player) =>
			{
                uint mouseData = 0;
                if (buttonPressed == MouseButton.XMB)
                {
                    if (_xmbButtonNumber == 0 || _xmbButtonNumber == 1)
                        mouseData = (uint)_xmbButtonNumber;
                    else
                        throw new ArgumentException("Аргумент xmbButton должен быть равен 0 или 1!");
                }
                else if (buttonPressed == MouseButton.WHEEL || buttonPressed == MouseButton.HWHEEL)
                {
                    mouseData = (uint)_mouseWheelMove;
                }
                uint inputFlags = (uint)((_doMove) ? 0x0001 : 0x0000);
                if (_isAbsolutePositioning)
                    inputFlags |= 0x8000;

                MOUSEINPUT mouseInput = new MOUSEINPUT()
                {
                    MouseData = mouseData,
                    Flags = inputFlags,
                    ExtraInfo = IntPtr.Zero,
                    Time = 0,
                    X = x,
                    Y = y
                };
                _inputs[0][0] = new  INPUT()
				{
					Type = (uint)INPUTTYPE.MOUSE,
					Data = new MOUSEKEYBDHARDWAREINPUT()
					{
						Mouse = new MOUSEINPUT()
                        {
                            MouseData = mouseInput.MouseData,
                            ExtraInfo = mouseInput.ExtraInfo,
                            Flags = (mouseInput.Flags & 0x8001) | ((uint)_buttonPressed),
                            X = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedX : X),
                            Y = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedY : Y),
                            Time = mouseInput.Time
                        }
                    }
				};

				SendInput(1, _inputs[0], Marshal.SizeOf(typeof(INPUT)));
				
				if (_doKeyUp && (uint)_buttonPressed > 0 && (uint)_buttonPressed < 0x0800)
				{
					if (_doKeyDown)
						await Task.Delay(32);
				
					_inputs[1][0] = new INPUT()
					{
						Type = (uint)INPUTTYPE.MOUSE,
						Data = new MOUSEKEYBDHARDWAREINPUT()
						{
							Mouse = new MOUSEINPUT()
							{
								MouseData = mouseInput.MouseData,
								ExtraInfo = mouseInput.ExtraInfo,
								Flags = (mouseInput.Flags & 0x8001) | ((uint)_buttonPressed << 1),
								X = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedX : X),
								Y = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedY : Y),
								Time = mouseInput.Time
							}
						}
					};
					SendInput(1, _inputs[1], Marshal.SizeOf(typeof(INPUT)));
				}
			};
		}

		private static int? _screenX = null;
		private static int? _screenY = null;
		public static int ScreenX 
		{ 
			get
			{
				if (!_screenX.HasValue)
				{
					_screenX = GetSystemMetrics(0);
				}
				return _screenX.Value;
			}
		}
		public static int ScreenY
		{
			get
			{
				if (!_screenY.HasValue)
				{
					_screenY = GetSystemMetrics(1);
				}
				return _screenY.Value;
			}
		}
		public static void ResetScreenResolutionCache()
		{
			_screenX = null;
			_screenY = null;
		}

		public MouseEventMacroElement(JsonElement macroJsonData)
		{
			bool doKeyDown = false, doKeyUp = false;
			MouseButton buttonPressed;
			bool doMove;
			bool isAbsolutePositioning;
			int x, y;
			int xmbButtonNumber = 0;
			int mouseWheelMove = 0;

			try
			{
				string flags = macroJsonData.GetProperty("flags").GetString();
                switch (flags)
                {
                    case "":
                        break;
                    case "D":
                        doKeyDown = true;
                        break;
                    case "U":
                        doKeyUp = true;
                        break;
                    case "DU":
                        doKeyDown = true;
                        doKeyUp = true;
                        break;
                    default:
                        throw new ArgumentException("Аргумент flags должен быть одним из следующих: U, D, DU, *пустая строка*");
                }
                buttonPressed = (MouseButton)macroJsonData.GetProperty("buttonPressed").GetUInt32();
				doMove = macroJsonData.GetProperty("doMove").GetBoolean();
				isAbsolutePositioning = macroJsonData.GetProperty("isAbsolutePositioning").GetBoolean();
				x = macroJsonData.GetProperty("x").GetInt32();
				y = macroJsonData.GetProperty("y").GetInt32();
				xmbButtonNumber = macroJsonData.GetProperty("xmbButtonNumber").GetInt32();
				mouseWheelMove = macroJsonData.GetProperty("mouseWheelMove").GetInt32();


                Initialize(doKeyDown, doKeyUp, buttonPressed, doMove, isAbsolutePositioning, x, y, xmbButtonNumber, mouseWheelMove);
            }
			catch (Exception ex)
			{
				throw new InvalidMacroTokenException("Ошибка загрузки файла макроса:\n" + ex.Message);
			}
        }

        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
			writer.WriteNumber("type", (byte)MacroType);
            writer.WriteString("flags", InputFlags);
			writer.WriteNumber("buttonPressed", (uint)_buttonPressed);
			writer.WriteBoolean("doMove", _doMove);
			writer.WriteBoolean("isAbsolutePositioning", _isAbsolutePositioning);
			writer.WriteNumber("x", _x);
			writer.WriteNumber("y", _y);
			writer.WriteNumber("xmbButtonNumber", _xmbButtonNumber);
			writer.WriteNumber("mouseWheelMove", _mouseWheelMove);
            writer.WriteEndObject();
        }
    }

	public class WaitTimeMacroElement : MacroElement
	{
		private long _msToWait;
		public long MsToWait
		{
			get => _msToWait;
			set => _msToWait = value;
		}

		public WaitTimeMacroElement(long msToWait)
		{
			_msToWait = msToWait;
            macroType = MacroType.WAITTIME;
			stringRepresentationFunc = () => $"Подождать {_msToWait} мс.";
			action = async (player) => await Task.Delay(TimeSpan.FromMilliseconds(msToWait));
		}

		public WaitTimeMacroElement(JsonElement macroJsonData)
		{
			_msToWait = macroJsonData.GetProperty("msToWait").GetInt64();
            macroType = MacroType.WAITTIME;
            stringRepresentationFunc = () => $"Подождать {_msToWait} мс.";
            action = async (player) => await Task.Delay(TimeSpan.FromMilliseconds(_msToWait));
        }
        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteNumber("type", (byte)macroType);
            writer.WriteNumber("msToWait", _msToWait);
            writer.WriteEndObject();
        }
    }
	public class LoopMacroElement : MacroElement
	{
		private long _condition;
		public long Condition
		{
			get => _condition;
			set => _condition = value;
		}
		public LoopMacroElement(long condition)
		{
            macroType = MacroType.LOOP;
			_condition = condition;
			stringRepresentationFunc = () => $"Повторить {_condition} раз";
			action = async (player) =>
			{
				player.PushIndexCondition(player.CurrentIndex, _condition);
			};
		}
		public LoopMacroElement(JsonElement macroJsonData)
		{
			_condition = macroJsonData.GetProperty("condition").GetInt64();
            macroType = MacroType.LOOP;
            stringRepresentationFunc = () => $"Повторить {_condition} раз";
            action = async (player) =>
            {
                player.PushIndexCondition(player.CurrentIndex, _condition);
            };
        }

        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteNumber("type", (byte)macroType);
			writer.WriteNumber("condition", _condition);
            writer.WriteEndObject();
        }
    }
	public class EndOfLoopMacroElement : MacroElement
	{
		public EndOfLoopMacroElement()
		{
            macroType = MacroType.END_OF_LOOP;
			stringRepresentationFunc = () => $"Конец цикла";
			action = async (player) =>
			{
				IndexCondition cond = player.PopIndex();
				cond.ConditionNum--;
				if (cond.ConditionNum != 0)
				{
					player.PushIndexCondition(cond.Index, cond.ConditionNum);
					player.CurrentIndex = cond.Index;
				}
			};
		}
		public EndOfLoopMacroElement(JsonElement macroJsonData)
		{
            macroType = MacroType.END_OF_LOOP;
            stringRepresentationFunc = () => $"Конец цикла";
            action = async (player) =>
            {
                IndexCondition cond = player.PopIndex();
                cond.ConditionNum--;
                if (cond.ConditionNum != 0)
                {
                    player.PushIndexCondition(cond.Index, cond.ConditionNum);
                    player.CurrentIndex = cond.Index;
                }
            };
        }
        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
			writer.WriteNumber("type", (byte)macroType);
			writer.WriteEndObject();
        }
    }

	[Serializable]
	public class InvalidMacroTokenException : Exception
	{
		public InvalidMacroTokenException() { }
		public InvalidMacroTokenException(string message) : base(message) { }
		public InvalidMacroTokenException(string message, Exception inner) : base(message, inner) { }
		protected InvalidMacroTokenException(
		  SerializationInfo info,
		  StreamingContext context) : base(info, context) { }
	}
}
