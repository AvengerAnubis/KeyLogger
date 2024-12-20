using SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public sealed partial class MacroElementEditorViewModel(IServiceProvider serviceProvider) : ObservableValidator
    {
        [ObservableProperty]
        private MacroElementViewModel? _elementViewModel;
        [ObservableProperty]
        private int _elementIndex;
        
        private MacroViewModel? _macroViewModel;
        public string MacroName
        {
            get
            {
                _macroViewModel ??= (MacroViewModel?)serviceProvider.GetService(typeof(MacroViewModel));
                if (_macroViewModel == null)
                    return "NULL";
                return _macroViewModel.MacroName;
            }
        }

        [RelayCommand]
        private void Save()
        {
            _macroViewModel ??= (MacroViewModel?)serviceProvider.GetService(typeof(MacroViewModel));
            if (_macroViewModel == null)
                return;
            try
            {
                _macroViewModel.SaveElement(ElementIndex, ElementViewModel!.MacroElement);
            }
            catch (Exception)
            {

            }
        }
    }
}
