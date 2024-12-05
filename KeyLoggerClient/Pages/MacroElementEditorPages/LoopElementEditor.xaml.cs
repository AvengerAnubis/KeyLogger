using SharpMacroPlayer.Macros;
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
