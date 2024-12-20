using SharpMacroPlayer.Bindings;
using SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public sealed partial class BindingViewModel() : ObservableObject
    {
        [ObservableProperty]
        private VkCodeViewModel _keyCode = new() { VkCode = "VK_W" };

        [ObservableProperty]
        private string _playCondition = "При нажатии";
        public string[] PlayConditions
        {
            get => [
                "При нажатии", "При удержании", "При отпускании"
            ];
        }

        [ObservableProperty]
        private MacroFile _macro = new(string.Empty);

        public Binding Binding
        {
            get => new(Enum.Parse<VK>(KeyCode.VkCode), Macro.Filename) 
            { 
                PlayCondition = PlayCondition switch
                {
                    "При нажатии" => BindingPlayCondition.ONKEYDOWN,
                    "При удержании" => BindingPlayCondition.ONKEYUP,
                    "При отпускании" => BindingPlayCondition.ONKEYHOLD,
                    _ => throw new ErrorException("Не найдено действие для PlayCondition!")
                }
            };
        }
    }
    public sealed partial class BindingsContainerViewModel(InputHooker hooker, MacroListViewModel macroListViewModel) : ObservableObject
    {
        [ObservableProperty]
        private MacroListViewModel _macroListViewModel = macroListViewModel;
        [ObservableProperty]
        private VkCodeViewModel _stopVkCode = new() { VkCode = "VK_SPACE" };
        [ObservableProperty]
        private ObservableCollection<BindingViewModel> _bindings = new(BindingLoaderSaver.LoadBindings("default.json")
            .Bindings.Select(e => new BindingViewModel()
            {
                KeyCode = new VkCodeViewModel() { VkCode = Enum.GetName(e.VkCode!.Value)! },
                Macro = new(e.MacroPath),
                PlayCondition = e.PlayCondition switch
                {
                    BindingPlayCondition.ONKEYDOWN => "При нажатии",
                    BindingPlayCondition.ONKEYUP => "При удержании",
                    BindingPlayCondition.ONKEYHOLD => "При отпускании",
                    _ => throw new ErrorException("Не найдено действие для PlayCondition!")
                }
            }));

        private BindingsPlayer _player = new(ref hooker);
        [ObservableProperty]
        private Visibility _playButtonVisibility = Visibility.Visible;
        [ObservableProperty]
        private Visibility _stopButtonVisibility = Visibility.Hidden;

        [RelayCommand]
        private void Add() => Bindings.Add(new BindingViewModel() 
        {
            KeyCode = new VkCodeViewModel() { VkCode = "VK_W" },
            Macro = new(MacroLoaderSaver.GetAllMacros()[0]),
            PlayCondition = "При нажатии"
        });
        [RelayCommand]
        private void Remove(BindingViewModel binding) => Bindings.Remove(binding);
        [RelayCommand]
        private void Save()
        {
            BindingContainer bindingContainer = new BindingContainer(Bindings.Select(e => e.Binding));
            BindingLoaderSaver.SaveBindings(bindingContainer, "default.json");
        }

        [RelayCommand]
        private void PlayStop()
        {
            if (_player.IsRunning)
            {
                _player.Stop();
                PlayButtonVisibility = Visibility.Visible;
                StopButtonVisibility = Visibility.Hidden;
                hooker.KeyInput -= StopKeyCheck;
            }
            else
            {
                _player.Bindings = new BindingContainer(Bindings.Select(e => e.Binding));
                _player.Start();
                PlayButtonVisibility = Visibility.Hidden;
                StopButtonVisibility = Visibility.Visible;
                hooker.KeyInput += StopKeyCheck;
            }
        }

        private void StopKeyCheck(object? sender, HookCallbackEventArgs args)
        {
            // Получаем структуру из памяти по указателю
            KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);

            if ((WM)args.wParam == WM.KEYDOWN || (WM)args.wParam == WM.SYSKEYDOWN)
            {
                if (data.VkCode == (ushort)Enum.Parse<VK>(StopVkCode.VkCode))
                {
                    PlayStop();
                }
            }
        }
    }
}
