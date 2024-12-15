using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMacroPlayer.Macros.MacroElements
{
    public class LoopMacroElement : MacroElement
    {
        public long Condition { get; set; }

        public LoopMacroElement(long condition)
        {
            Condition = condition;
            MacroType = MacroType.LOOP;
        }
        public LoopMacroElement(JsonElement macroJsonData)
        {
            Condition = macroJsonData.GetProperty("condition").GetInt64();
            MacroType = MacroType.LOOP;
        }

        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteNumber("type", (byte)MacroType);
            writer.WriteNumber("condition", Condition);
            writer.WriteEndObject();
        }

        public override async Task Execute(MacroPlayer player) => player.PushIndexCondition(player.CurrentIndex, Condition);
        public override string ToString() => $"Повторить {Condition} раз";
    }
}
