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
    /// Логика взаимодействия для LoopElementEditor.xaml
    /// </summary>
    public partial class LoopElementEditor : Page
    {
        public event EventHandler<MacroElement> OkPressed;

        public string Condition { get; set; }

        public LoopElementEditor(MacroElement el)
        {
            LoopMacroElement element = el as LoopMacroElement;
            Condition = element.Condition.ToString();
            InitializeComponent();
            this.DataContext = element;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            long cond = 0;
            if (long.TryParse(textBox.Text, out cond))
            {
                MacroElement macroElement = new LoopMacroElement(cond);
                OkPressed?.Invoke(this, macroElement);
            }
        }
    }
}
