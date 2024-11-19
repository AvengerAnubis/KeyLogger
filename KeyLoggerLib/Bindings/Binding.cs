using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected bool isForKeyboard;
        public bool IsForKeyboard
        {
            get => isForKeyboard;
            set => isForKeyboard = value;
        }

        protected BindingPlayCondition playCondition;
        public BindingPlayCondition PlayCondition
        {
            get => playCondition;
            set => playCondition = value;
        }

        protected bool isRunning;
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

        protected long holdTime;
        public long HoldTime
        {
            get => holdTime;
            set => holdTime = value;
        }

        protected bool isKeyPressed;

        protected MacroPlayer player;

        public Binding(VK vkCode, Macro macro = null)
        {
            this.vkCode = vkCode;
            this.macro = macro;
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
                        if (playCondition == BindingPlayCondition.ONKEYDOWN)
                        {
                            shouldExecute = true;
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
            if (!isRunning && !player.IsRunning)
            {
                isRunning = true;
                player.Macro = macro;

                player.RunMacroAsync().ContinueWith((task) => { isRunning = false; });
            }
        }
    }
}
