using SharpMacroPlayer.ClientNew.ViewModels;
using System.Windows.Controls;

namespace SharpMacroPlayer.ClientNew.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacroListPage.xaml
    /// </summary>
    public partial class MacroEditorPage : Page
    {
        public MacroViewModel ViewModel { get; }

        public MacroEditorPage(MacroViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
            ViewModel.SetMacro(MacroLoaderSaver.GetAllMacros()[0]);
        }
    }
}