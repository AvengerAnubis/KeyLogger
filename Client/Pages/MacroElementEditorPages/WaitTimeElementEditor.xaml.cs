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
