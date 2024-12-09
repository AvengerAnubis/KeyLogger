using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMacroPlayer.Macros.MacroElements
{
    public class EndOfLoopMacroElement : MacroElement
    {
        public EndOfLoopMacroElement()
        {
            MacroType = MacroType.END_OF_LOOP;
        }
        public EndOfLoopMacroElement(JsonElement macroJsonData)
        {
            MacroType = MacroType.END_OF_LOOP;
        }
        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteNumber("type", (byte)MacroType);
            writer.WriteEndObject();
        }

        public override async Task Execute(MacroPlayer player)
        {
            IndexCondition cond = player.PopIndex();
            cond.ConditionNum--;
            if (cond.ConditionNum != 0)
            {
                player.PushIndexCondition(cond.Index, cond.ConditionNum);
                player.CurrentIndex = cond.Index;
            }
        }
        public override string ToString() => $"Конец цикла";
    }
}
