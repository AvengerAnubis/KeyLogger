using SharpMacroPlayer.ClientNew.Views.Pages;
using System.Collections.ObjectModel;
using System.IO;
using Wpf.Ui;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public partial class MacroFile(string macroFile) : ObservableObject
    {
        [ObservableProperty]
        private string _filename = macroFile;
        public string FileNameWithoutExtension { get => Path.GetFileNameWithoutExtension(Filename); }
    }
    public sealed partial class MacroListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MacroFile> _macroFiles = new(MacroLoaderSaver.GetAllMacros().Select(e => new MacroFile(e)));
        [ObservableProperty]
        private string _newMacroName = string.Empty;

        private MacroViewModel _macroViewModel;
        private INavigationWindow _navigationView;

        [RelayCommand]
        private void Delete(string file)
        {
            MacroLoaderSaver.DeleteMacro(file);
            Update();
        }
        [RelayCommand]
        private void Add()
        {
            MacroLoaderSaver.SaveMacros(new(), NewMacroName + ".json");
            Update();
        }
        [RelayCommand]
        private void Edit(string file)
        {
            _macroViewModel.SetMacro(file);
            _navigationView.Navigate(typeof(MacroEditorPage));
        }
        [RelayCommand]
        private void Update() => MacroFiles = new(MacroLoaderSaver.GetAllMacros().Select(e => new MacroFile(e)));

        public MacroListViewModel(MacroViewModel macroViewModel, INavigationWindow navigationView)
        {
            _macroViewModel = macroViewModel;
            _navigationView = navigationView;
        }
    }
}