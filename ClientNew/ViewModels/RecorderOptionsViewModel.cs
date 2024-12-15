using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public sealed partial class RecorderOptionsViewModel : ObservableValidator
    {
        [ObservableProperty]
        private bool _recordIntermediateMouseMovement;
        [ObservableProperty]
        private string _mouseMovementRecordingInterval = string.Empty;

        [ObservableProperty]
        private bool _saveDelayBetweenActions;
        partial void OnSaveDelayBetweenActionsChanged(bool value) => DoNotSaveDelayBetweenAction = !value;
        [ObservableProperty]
        private bool _doNotSaveDelayBetweenAction;
        [ObservableProperty]
        private string _defaultDelay = string.Empty;

        public MacroRecorderOptions OptionsStruct
        {
            get
            {
                try
                {
                    uint mouseRecordingInterval = 0, defaultDelay = 0;
                    if (RecordIntermediateMouseMovement)
                        mouseRecordingInterval = uint.Parse(MouseMovementRecordingInterval);
                    if (DoNotSaveDelayBetweenAction)
                        defaultDelay = uint.Parse(DefaultDelay);
                    return new MacroRecorderOptions
                    {
                        RecordIntermediateMouseMovement = RecordIntermediateMouseMovement,
                        MouseMovementRecordingInterval = mouseRecordingInterval,
                        SaveDelayBetweenActions = SaveDelayBetweenActions,
                        DefaultDelay = defaultDelay
                    };
                }
                catch
                {
                    throw new ErrorException("Невозможно получить структуры настроек: некорректные данные");
                }
            }
        }
    }
}
