using SharpMacroPlayer.ClientNew.ViewModels;
using System.Windows.Controls;

namespace SharpMacroPlayer.ClientNew.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для BindingEditorPage.xaml
    /// </summary>
    public partial class BindingEditorPage : Page
    {
        public BindingsContainerViewModel ViewModel { get; }


        public BindingEditorPage(BindingsContainerViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }
    }
}