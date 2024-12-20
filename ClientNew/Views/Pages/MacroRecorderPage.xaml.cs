using SharpMacroPlayer.ClientNew.ViewModels;
using System.Windows.Controls;

namespace SharpMacroPlayer.ClientNew.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacroRecorderPage.xaml
    /// </summary>
    public partial class MacroRecorderPage : Page
    {
        public RecorderViewModel ViewModel { get; }

        public MacroRecorderPage(RecorderViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }
    }
}