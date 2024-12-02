using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
#endregion

namespace KeyLogger.Bindings
{
    public enum BindingPlayCondition
    {
        ONKEYDOWN,
        ONKEYUP,
        ONKEYHOLD
    }
    /// <summary>
    /// Класс для биндинга определённого макроса на определённую клавишу
    /// </summary>
    public class Binding
    {
        protected bool isForKeyboard = true;
        public bool IsForKeyboard
        {
            get => isForKeyboard;
            set => isForKeyboard = value;
        }

        protected BindingPlayCondition playCondition = BindingPlayCondition.ONKEYDOWN;
        public BindingPlayCondition PlayCondition
        {
            get => playCondition;
            set => playCondition = value;
        }

        protected bool isRunning = false;
        /// <summary>
        /// Проигрывается ли макрос в текущее время?
        /// </summary>
        public bool IsRunning
        {
            get => isRunning;
        }

        protected Macro macro;
        /// <summary>
        /// Макрос, который будет выполняться по нажатию клавиши
        /// </summary>
        public Macro Macro
        {
            get => macro;
            set => macro = value;
        }

        protected string macroPath;
        /// <summary>
        /// Путь к макросу
        /// </summary>
        public string MacroPath
        {
            get => macroPath;
            set => macroPath = value;
        }

        protected VK? vkCode;
        /// <summary>
        /// Код клавиши, по нажатию на которую будет выполнен макрос
        /// </summary>
        public VK? VkCode
        {
            get => vkCode;
            set => vkCode = value;
        }

        protected MouseButton? mouseButton;
        public MouseButton? MouseButton
        {
            get => mouseButton;
            set => mouseButton = value;
        }

        protected long holdTime = 1000;
        public long HoldTime
        {
            get => holdTime;
            set => holdTime = value;
        }

        protected bool isKeyPressed = false;
        protected Task holdTask;
        protected CancellationTokenSource cancelSource;

        protected MacroPlayer player;

        public Binding(VK vkCode, Macro macro = null)
        {
            this.vkCode = vkCode;
            this.macroPath = "!notsaved";
            this.macro = macro;
            player = new MacroPlayer(macro);
        }
        public Binding(VK vkCode, string macroPath = null)
        {
            this.vkCode = vkCode;
            this.macroPath = macroPath;
            this.macro = MacroLoaderSaver.LoadMacro(macroPath);
            player = new MacroPlayer(macro);
        }
        public Binding(JsonElement macroJsonData)
        {
            isForKeyboard = macroJsonData.GetProperty("isForKeyboard").GetBoolean();
            playCondition = (BindingPlayCondition)macroJsonData.GetProperty("playCondition").GetByte();
            macroPath = macroJsonData.GetProperty("macroPath").GetString();
            if (isForKeyboard)
            {
                vkCode = (VK)macroJsonData.GetProperty("vkCode").GetUInt16();
            }
            else
            {
                mouseButton = (MouseButton)macroJsonData.GetProperty("mouseButton").GetUInt32();
            }
            if (playCondition == BindingPlayCondition.ONKEYHOLD)
            {
                holdTime = macroJsonData.GetProperty("holdTime").GetInt64();
            }
            player = new MacroPlayer(macro);
        }

        public void ExecuteIfVkMatches(VK vkCode, bool isKeyDown)
        {
            if (isForKeyboard)
            {
                bool shouldExecute = false;
                if (this.vkCode.HasValue && this.vkCode.Value == vkCode)
                {
                    if (isKeyDown)
                    {
                        if (!isKeyPressed)
                        {
                            isKeyPressed = true;
                            if (playCondition == BindingPlayCondition.ONKEYDOWN)
                            {
                                shouldExecute = true;
                            }
                            else if (playCondition == BindingPlayCondition.ONKEYHOLD)
                            {
                                cancelSource = new CancellationTokenSource();
                                CancellationToken token = cancelSource.Token;
                                holdTask = Task.Run(async () =>
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(holdTime));
                                    if (!token.IsCancellationRequested)
                                    {
                                        Execute();
                                    }
                                }, token);
                            }
                        }
                    }
                    else
                    {
                        isKeyPressed = false;
                        if (playCondition == BindingPlayCondition.ONKEYUP)
                        {
                            shouldExecute = true;
                        }
                        else if (playCondition == BindingPlayCondition.ONKEYHOLD)
                        {
                            cancelSource.Cancel();
                        }
                    }
                }
                if (shouldExecute)
                    Execute();
            }
        }
        public void ExecuteIfMouseButtonMatches(MouseButton mouseButton, bool isKeyDown)
        {
            if (!isForKeyboard)
            {
                if (this.mouseButton.HasValue && this.mouseButton.Value == mouseButton)
                {
                    Execute();
                }
            }
        }

        public void Execute()
        {
            if (!isRunning && !player.IsRunning && (macro == null || (macroPath != null && macroPath != "!notsaved")))
            {
                isRunning = true;
                if (macroPath != "!notsaved")
                {
                    macro = MacroLoaderSaver.LoadMacro(MacroPath);
                }
                player.Macro = macro;

                player.RunMacroAsync().ContinueWith((task) => { isRunning = false; });
            }
        }

        public void WriteToJson(ref Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("isForKeyboard", isForKeyboard);
            writer.WriteNumber("playCondition", (byte)playCondition);
            writer.WriteString("macroPath", macroPath);
            if (isForKeyboard)
            {
                writer.WriteNumber("vkCode", (ushort)vkCode);
            }
            else
            {
                writer.WriteNumber("mouseButton", (uint)mouseButton);
            }
            if (playCondition == BindingPlayCondition.ONKEYHOLD)
            {
                writer.WriteNumber("holdTime", holdTime);
            }
            writer.WriteEndObject();
        }
    }
}
