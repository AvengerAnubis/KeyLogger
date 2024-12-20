using SharpMacroPlayer.ClientNew.ViewModels;
using System.Windows.Controls;

namespace SharpMacroPlayer.ClientNew.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacroListPage.xaml
    /// </summary>
    public partial class MacroListPage : Page
    {
        public MacroListViewModel ViewModel { get; }

        public MacroListPage(MacroListViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }
    }
}