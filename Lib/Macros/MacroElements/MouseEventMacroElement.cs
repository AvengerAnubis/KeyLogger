using System;
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

        public MouseButton ButtonPressed { get; set; }

        public bool DoMove { get; set; }

        public bool IsAbsolutePositioning { get; set; }

        public int X { get; set; }
        public int NormalizedX
        {
            get => (int)((double)X / (double)ScreenX * (double)ushort.MaxValue);
        }

        public int Y { get; set; }
        public int NormalizedY
        {
            get => (int)((double)Y / (double)ScreenY * (double)ushort.MaxValue);
        }

        public int XmbButtonNumber { get; set; }
        public int MouseWheelMove {  get; set; }

        public bool DoKeyDown {  get; set; }
        public bool DoKeyUp { get; set; }

        public string InputFlags
        {
            get => ((DoKeyDown) ? "D" : "") + ((DoKeyUp) ? "U" : "");
        }

        public MouseEventMacroElement(bool doKeyDown, bool doKeyUp, MouseButton buttonPressed, bool doMove, bool isAbsolutePositioning, int x, int y, int xmbButtonNumber = 0, int mouseWheelMove = 0) => Initialize(doKeyDown, doKeyUp, buttonPressed, doMove, isAbsolutePositioning, x, y, xmbButtonNumber, mouseWheelMove);
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
            MacroType = MacroType.MOUSE_EVENT;
            _inputs[0] = new INPUT[1];
            _inputs[1] = new INPUT[1];
            ButtonPressed = buttonPressed;
            DoKeyDown = doKeyDown;
            DoKeyUp = doKeyUp;
            DoMove = doMove;
            IsAbsolutePositioning = isAbsolutePositioning;
            X = x;
            Y = y;
            XmbButtonNumber = xmbButtonNumber;
            MouseWheelMove = mouseWheelMove;
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
            writer.WriteNumber("buttonPressed", (uint)ButtonPressed);
            writer.WriteBoolean("doMove", DoMove);
            writer.WriteBoolean("isAbsolutePositioning", IsAbsolutePositioning);
            writer.WriteNumber("x", X);
            writer.WriteNumber("y", Y);
            writer.WriteNumber("xmbButtonNumber", XmbButtonNumber);
            writer.WriteNumber("mouseWheelMove", MouseWheelMove);
            writer.WriteEndObject();
        }

        public override async Task Execute(MacroPlayer player)
        {
            uint mouseData = 0;
            if (ButtonPressed == MouseButton.XMB)
            {
                if (XmbButtonNumber == 0 || XmbButtonNumber == 1)
                    mouseData = (uint)XmbButtonNumber;
                else
                    throw new ArgumentException("Аргумент xmbButton должен быть равен 0 или 1!");
            }
            else if (ButtonPressed == MouseButton.WHEEL || ButtonPressed == MouseButton.HWHEEL)
            {
                mouseData = (uint)MouseWheelMove;
            }
            uint inputFlags = (uint)((DoMove) ? 0x0001 : 0x0000);
            if (IsAbsolutePositioning)
                inputFlags |= 0x8000;

            MOUSEINPUT mouseInput = new MOUSEINPUT()
            {
                MouseData = mouseData,
                Flags = inputFlags,
                ExtraInfo = IntPtr.Zero,
                Time = 0,
                X = X,
                Y = X
            };
            _inputs[0][0] = new INPUT()
            {
                Type = (uint)INPUTTYPE.MOUSE,
                Data = new MOUSEKEYBDHARDWAREINPUT()
                {
                    Mouse = new MOUSEINPUT()
                    {
                        MouseData = mouseInput.MouseData,
                        ExtraInfo = mouseInput.ExtraInfo,
                        Flags = (mouseInput.Flags & 0x8001) | ((uint)ButtonPressed),
                        X = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedX : X),
                        Y = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedY : Y),
                        Time = mouseInput.Time
                    }
                }
            };

            SendInput(1, _inputs[0], Marshal.SizeOf(typeof(INPUT)));

            if (DoKeyUp && (uint)ButtonPressed > 0 && (uint)ButtonPressed < 0x0800)
            {
                if (DoKeyDown)
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
                            Flags = (mouseInput.Flags & 0x8001) | ((uint)ButtonPressed << 1),
                            X = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedX : X),
                            Y = ((mouseInput.Flags & (uint)MOUSEEVENTF.ABSOLUTE) > 0 ? NormalizedY : Y),
                            Time = mouseInput.Time
                        }
                    }
                };
                SendInput(1, _inputs[1], Marshal.SizeOf(typeof(INPUT)));
            }
        }

        public override string ToString()
        {
            string representation = string.Empty;
            if (DoKeyUp && !DoKeyDown)
                representation = "Отпустить";
            else if (!DoKeyUp && DoKeyDown)
                representation = "Удерживать";
            else
                representation = "Нажать";

            representation += ": ";
            switch (ButtonPressed)
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
                    representation += $"Кнопка {(XmbButtonNumber == 0 ? "4" : "5")}";
                    break;
                case MouseButton.WHEEL:
                    representation = $"Колесо мыши {(MouseWheelMove > 0 ? "Вверх" : "Вниз")}";
                    break;
                case MouseButton.HWHEEL:
                    representation = $"Колесо мыши {(MouseWheelMove > 0 ? "Вправо" : "Влево")}";
                    break;
                default:
                    break;
            }
            if (DoMove && ButtonPressed != MouseButton.NOTHING)
            {
                representation += $"; X = {X}, Y = {Y} ({(IsAbsolutePositioning ? "абсолютные" : "относительные")})";
            }
            return representation;
        }
    }
}
