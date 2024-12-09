using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Macros.MacroElements;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
using MouseButton = SharpMacroPlayer.Macros.MacroElements.MouseButton;
#endregion

namespace SharpMacroPlayer.Pages.MacroElementEditorPages
{
    /// <summary>
    /// Логика взаимодействия для MouseElementEditor.xaml
    /// </summary>
    public partial class MouseElementEditor : Page, INotifyPropertyChanged
    {
        public event EventHandler<MacroElement> OkPressed;
        public event PropertyChangedEventHandler PropertyChanged;

        public string SelectedMouseButton { get; set; }
        public bool DoMove { get; set; }
        public bool IsAbsolute { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string XmbButtonNumber { get; set; }
        public string MouseWheelMove { get; set; }
        private bool _doKeyDown;
        public bool DoKeyDown
        {
            get => _doKeyDown;
            set
            {
                _doKeyDown = value;
                if (!_doKeyDown && !_doKeyUp)
                {
                    DoKeyUp = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DoKeyDown"));
            }
        }
        private bool _doKeyUp;
        public bool DoKeyUp
        {
            get => _doKeyUp;
            set
            {
                _doKeyUp = value;
                if (!_doKeyUp && !_doKeyDown)
                {
                    DoKeyDown = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DoKeyUp"));
            }
        }



        public string[] AllXmbButtonNumbers { get => new string[] { "1", "2" }; }
        public string[] AllMouseButtons { get => Enum.GetNames(typeof(MouseButton)); }

        public MouseElementEditor(MacroElement el)
        {
            MouseEventMacroElement element = el as MouseEventMacroElement;
            SelectedMouseButton = Enum.GetName(typeof(MouseButton), element.ButtonPressed);
            DoMove = element.DoMove;
            IsAbsolute = element.IsAbsolutePositioning;
            X = element.X.ToString();
            Y = element.Y.ToString();
            XmbButtonNumber = element.XmbButtonNumber.ToString();
            MouseWheelMove = element.MouseWheelMove.ToString();
            DoKeyDown = element.DoKeyDown;
            DoKeyUp = element.DoKeyUp;
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int x = 0, y = 0, xmb = 0, wheel = 0;
            if (int.TryParse(X, out x) && int.TryParse(Y, out y) && int.TryParse(XmbButtonNumber, out xmb) && int.TryParse(MouseWheelMove, out wheel))
            {
                MacroElement macroElement = new MouseEventMacroElement(DoKeyDown, DoKeyUp, (MouseButton)Enum.Parse(typeof(MouseButton), SelectedMouseButton), DoMove, IsAbsolute, x, y, xmb, wheel);
                OkPressed?.Invoke(this, macroElement);
            }
        }
    }
}
