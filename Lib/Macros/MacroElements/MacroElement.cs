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
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Macros.MacroElements
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
        public MacroType MacroType { get; protected set; }

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
        public abstract Task Execute(MacroPlayer player);
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
