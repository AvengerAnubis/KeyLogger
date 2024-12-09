using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

using MouseButton = SharpMacroPlayer.Macros.MacroElements.MouseButton;

namespace SharpMacroPlayer.UserControls
{
	/// <summary>
	/// Логика взаимодействия для MouseButton.xaml
	/// </summary>
	public partial class MouseKeyButton : Button
	{
		protected MouseButton mouseButton;
		public MouseButton MouseButton
		{
			get => mouseButton;
			set => mouseButton = value;
		}

		protected InputHooker inputHooker;
		public InputHooker InputHooker
		{
			get => inputHooker;
			set
			{
				if (InputHooker != null)
				{
					inputHooker.MouseInput -= InputHooker_MouseInput;
				}
				inputHooker = value;
				inputHooker.MouseInput += InputHooker_MouseInput;
			}
		}

		public Color UnpressedColor { get; set; }
		public Color PressedColor { get; set; }

		private void InputHooker_MouseInput(object sender, HookCallbackEventArgs args)
		{
            // Получаем структуру из памяти по указателю
            MSLLHOOKSTRUCT data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(args.lParam);

            bool isKeyDown;
            MouseButton mouseButton;

            switch ((WM)args.wParam)
            {
                case WM.LBUTTONDOWN:
                    isKeyDown = true;
                    mouseButton = MouseButton.LMB;
                    break;
                case WM.LBUTTONUP:
                    isKeyDown = false;
                    mouseButton = MouseButton.LMB;
                    break;
                case WM.RBUTTONDOWN:
                    isKeyDown = true;
                    mouseButton = MouseButton.RMB;
                    break;
                case WM.RBUTTONUP:
                    isKeyDown = false;
                    mouseButton = MouseButton.RMB;
                    break;
                case WM.XBUTTONDOWN:
                    isKeyDown = true;
                    mouseButton = MouseButton.XMB;
                    break;
                case WM.XBUTTONUP:
                    isKeyDown = false;
                    mouseButton = MouseButton.XMB;
                    break;
                default:
                    return;
            }

			if (mouseButton == this.mouseButton)
			{
				this.Background = new SolidColorBrush((isKeyDown) ? PressedColor : UnpressedColor);
			}
		}

		public MouseKeyButton()
		{
			InitializeComponent();
		}
	}
}
