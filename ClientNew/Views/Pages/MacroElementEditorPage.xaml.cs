using SharpMacroPlayer.ClientNew.ViewModels;
using SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels;
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
using Wpf.Ui;
using Wpf.Ui.Controls;
using MouseButton = SharpMacroPlayer.Macros.MacroElements.MouseButton;

namespace SharpMacroPlayer.ClientNew.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacroElementEditorView.xaml
    /// </summary>
    public partial class MacroElementEditorPage : Page
    {
        public MacroElementEditorViewModel ViewModel { get; }

        public MacroElementEditorPage(MacroElementEditorViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }
    }
}
