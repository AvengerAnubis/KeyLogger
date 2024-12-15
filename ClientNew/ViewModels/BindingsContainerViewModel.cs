using SharpMacroPlayer.Bindings;
using SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels
{
    public sealed partial class BindingViewModel : ObservableObject
    {
        [ObservableProperty]
        private VkCodeViewModel _keyCode;

        [ObservableProperty]
        private string _playCondition = "При нажатии";
        public string[] PlayConditions
        {
            get => [
                "При нажатии", "При удержании", "При отпускании"
            ];
        }

        [ObservableProperty]
        private string _macro = string.Empty;
        public string[] Macros
        {
            get => MacroLoaderSaver.GetAllMacros().Select(e => Path.GetFileNameWithoutExtension(e)).ToArray();
        }

        public Binding Binding
        {
            get => new(Enum.Parse<VK>(KeyCode.VkCode), Macro + ".json") 
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
    public sealed partial class BindingsContainerViewModel(InputHooker hooker) : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BindingViewModel> _bindings = new(BindingLoaderSaver.LoadBindings("default.json")
            .Bindings.Select(e => new BindingViewModel()
            {
                KeyCode = new VkCodeViewModel() { VkCode = Enum.GetName(e.VkCode!.Value)! },
                Macro = Path.GetFileNameWithoutExtension(e.MacroPath),
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
            Macro = MacroLoaderSaver.GetAllMacros()[0],
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
            }
            else
            {
                _player.Bindings = new BindingContainer(Bindings.Select(e => e.Binding));
                _player.Start();
                PlayButtonVisibility = Visibility.Hidden;
                StopButtonVisibility = Visibility.Visible;
            }
        }
    }
}
