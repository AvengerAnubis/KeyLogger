using System;
using System.Windows;
using System.Windows.Controls;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Macros.MacroElements;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Pages.MacroElementEditorPages
{
    /// <summary>
    /// Логика взаимодействия для WaitTimeElementEditor.xaml
    /// </summary>
    public partial class WaitTimeElementEditor : Page
    {
        public event EventHandler<MacroElement> OkPressed;

        public string MsToWait {  get; set; }

        public WaitTimeElementEditor(MacroElement el)
        {
            WaitTimeMacroElement element = el as WaitTimeMacroElement;
            MsToWait = element.MsToWait.ToString();
            InitializeComponent();
            this.DataContext = element;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            long ms = 0;
            if (long.TryParse(textBox.Text, out ms))
            { 
                MacroElement macroElement = new WaitTimeMacroElement(ms);
                OkPressed?.Invoke(this, macroElement);
            }
        }
    }
}
