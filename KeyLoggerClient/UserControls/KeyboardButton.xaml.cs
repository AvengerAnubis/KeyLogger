using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
using System.Runtime.InteropServices;
#endregion

namespace KeyLogger.UserControls
{
    /// <summary>
    /// Логика взаимодействия для KeyboardButton.xaml
    /// </summary>
    public partial class KeyboardButton : Button
    {
        protected VK keyCode;
        public VK KeyCode
        {
            get => keyCode;
            set => keyCode = value;
        }

        protected InputHooker inputHooker;
        public InputHooker InputHooker
        {
            get => inputHooker;
            set
            {
                if (InputHooker != null)
                {
                    inputHooker.KeyInput -= InputHooker_KeyInput;
                }
                inputHooker = value;
                inputHooker.KeyInput += InputHooker_KeyInput;
            }
        }

        public Color UnpressedColor { get; set; }
        public Color PressedColor { get; set; }

        private void InputHooker_KeyInput(object sender, HookCallbackEventArgs args)
        {
            // Получаем структуру из памяти по указателю
            KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);

            if (KeyCode == (VK)data.VkCode)
            {
                bool isKeyDown;
                switch ((WM)args.wParam)
                {
                    case WM.KEYDOWN:
                        isKeyDown = true;
                        break;
                    case WM.KEYUP:
                        isKeyDown = false;
                        break;
                    case WM.SYSKEYDOWN:
                        isKeyDown = true;
                        break;
                    case WM.SYSKEYUP:
                        isKeyDown = false;
                        break;
                    default:
                        return;
                }
    
                this.Background = new SolidColorBrush((isKeyDown) ? PressedColor : UnpressedColor);
            }
        }

        public KeyboardButton()
        {
            InitializeComponent();
        }
    }
}
