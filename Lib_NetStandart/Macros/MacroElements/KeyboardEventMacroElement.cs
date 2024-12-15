using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.Json;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Macros.MacroElements
{
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

        public bool DoKeyDown { get; set; }
        public bool DoKeyUp { get; set; }

        public string InputFlags
        {
            get => ((DoKeyDown) ? "D" : "") + ((DoKeyUp) ? "U" : "");
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
            MacroType = MacroType.KEYBOARD_EVENT;
            _keys = keyInputs;
            DoKeyDown = doKeyDown;
            DoKeyUp = doKeyUp;
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
            writer.WriteNumber("type", (byte)MacroType);
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

        public override async Task Execute(MacroPlayer player)
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
            if (DoKeyDown)
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

            if (DoKeyUp)
            {
                if (DoKeyDown)
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
        }
        public override string ToString()
        {
            string representation = string.Empty;
            if (DoKeyUp && !DoKeyDown)
                representation = "Отпустить";
            else if (!DoKeyUp && DoKeyDown)
                representation = "Удерживать";
            else if (DoKeyUp && DoKeyDown)
                representation = "Нажать";
            else
                return "Действие не задано";

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
        }
    }
}
