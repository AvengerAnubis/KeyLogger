using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMacroPlayer.Macros.MacroElements
{
    public class WaitTimeMacroElement : MacroElement
    {
        public long MsToWait { get; set; }


        public WaitTimeMacroElement(long msToWait)
        {
            MsToWait = msToWait;
            MacroType = MacroType.WAITTIME;
        }

        public WaitTimeMacroElement(JsonElement macroJsonData)
        {
            MsToWait = macroJsonData.GetProperty("msToWait").GetInt64();
            MacroType = MacroType.WAITTIME;
        }
        public override void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteNumber("type", (byte)MacroType);
            writer.WriteNumber("msToWait", MsToWait);
            writer.WriteEndObject();
        }

        public override async Task Execute(MacroPlayer player)
        {
            long timeToWait = MsToWait - (player.Stopwatch.ElapsedMilliseconds - player.TimeSinceStart);
            if (timeToWait > 0)
                await Task.Delay(TimeSpan.FromMilliseconds(timeToWait));
            player.TimeSinceStart = player.Stopwatch.ElapsedMilliseconds;
        }
        public override string ToString() => $"Подождать {MsToWait} мс.";
    }
}
