using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
#endregion

namespace KeyLogger.Macros
{
    public class Macro
    {
        private List<MacroElement> _macroElements;
        public ref List<MacroElement> MacroElements
        {
            get => ref _macroElements;
        }

        public Macro()
        {
            _macroElements = new List<MacroElement>();
        }
        public Macro(List<MacroElement> macroElements)
        {
            _macroElements = macroElements;
        }
        public Macro(JsonDocument macroJsonData)
        {
            _macroElements = new List<MacroElement>();
            JsonElement.ArrayEnumerator jsonArray = macroJsonData.RootElement.EnumerateArray();
            foreach (var macroElement in jsonArray)
            {
                _macroElements.Add(MacroElement.CreateFromJson(macroElement));
            }
        }
        public void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (MacroElement macroElement in _macroElements)
            {
                macroElement.WriteToJson(ref writer);
            }
            writer.WriteEndArray();
        }
    }
}
