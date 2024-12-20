using SharpMacroPlayer.ClientNew.ViewModels;
using System.Windows.Controls;

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