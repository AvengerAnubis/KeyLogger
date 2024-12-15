using SharpMacroPlayer.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public sealed partial class RecorderViewModel(InputHooker hooker, RecorderOptionsViewModel optionsViewModel, IServiceProvider serviceProvider) : ObservableObject
    {
        [ObservableProperty]
        private string _macroName = string.Empty;
        [ObservableProperty]
        private RecorderOptionsViewModel _optionsViewModel = optionsViewModel;

        [ObservableProperty]
        private Visibility _playButtonVisibility = Visibility.Visible;
        [ObservableProperty]
        private Visibility _stopButtonVisibility = Visibility.Hidden;

        private MacroRecorder _macroRecorder = new(ref hooker);


        [RelayCommand]
        private void PlayStop()
        {
            if (_macroRecorder.IsRecording)
            {
                _macroRecorder.EndRecording();
                PlayButtonVisibility = Visibility.Visible;
                StopButtonVisibility = Visibility.Hidden;
                MacroLoaderSaver.SaveMacros(_macroRecorder.Macro, MacroName + ".json");
                ((MacroListViewModel)serviceProvider.GetService(typeof(MacroListViewModel))!).UpdateCommand.Execute(null);
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(MacroName))
                        throw new ErrorException("Название файла не может быть пустым!");
                    MacroRecorderOptions options = OptionsViewModel.OptionsStruct;
                    _macroRecorder.BeginRecording(options);
                    PlayButtonVisibility = Visibility.Hidden;
                    StopButtonVisibility = Visibility.Visible;
                }
                catch { }
            }
        }
    }
}
