using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public partial class MainWindowViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string _title = $"SharpMacroPlayer";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems =
        [
            new NavigationViewItem()
            {
                Content = "Макросы",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentText24 },
                TargetPageType = typeof(Views.Pages.MacroListPage)
            },
            new NavigationViewItem()
            {
                Content = "Биндинги",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Keyboard24 },
                TargetPageType = typeof(Views.Pages.BindingEditorPage)
            },
            new NavigationViewItem()
            {
                Content = "Запись макросов",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Edit24 },
                TargetPageType = typeof(Views.Pages.MacroRecorderPage)
            }
        ];
    }
}
