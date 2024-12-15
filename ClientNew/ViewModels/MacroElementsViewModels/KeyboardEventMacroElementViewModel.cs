using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class KeyboardEventMacroElementViewModel : MacroElementViewModel
    {
        [ObservableProperty]
        private ObservableCollection<VkCodeViewModel> _keyCodes = new ObservableCollection<VkCodeViewModel>();

        [RelayCommand]
        private void AddKeyCode() => KeyCodes.Add(new());
        [RelayCommand]
        private void RemoveKeyCode() 
        {
            if (KeyCodes.Count > 1) KeyCodes.Remove(KeyCodes.Last());
        }

        [ObservableProperty]
        private string _keyState = string.Empty;
        public string[] KeyStates { get => ["Нажать", "Удержать", "Отпустить", "Ничего"]; }


        public override MacroElement GetMacroElement()
        {
            ValidateAllProperties();
            if (HasErrors)
                throw new ValidationException("Неверные значения!");
            bool[] keyStates = KeyState switch
            {
                "Нажать" => [true, true],
                "Удержать" => [true, false],
                "Отпустить" => [false, true],
                "Ничего" => [false, false],
                _ => throw new ErrorException("Ничего не использовалось")
            };
            return new KeyboardEventMacroElement(
                KeyCodes.Select(e => (ushort)Enum.Parse<VK>(e.VkCode)).ToArray(),
                KeyCodes.Select(e => KeyEventType.VIRTUAL_KEY).ToArray(),
                keyStates[0], keyStates[1]
            );
        }

        public override void SetMacroElement(MacroElement macroElement)
        {
            if (macroElement is KeyboardEventMacroElement element)
            {
                KeyCodes.Clear();
                foreach (ushort vkCode in element.KeyCodes)
                {
                    KeyCodes.Add(new VkCodeViewModel() { VkCode = Enum.GetName((VK)vkCode)! });
                }
                KeyState = element.InputFlags switch
                {
                    "D" => "Удержать",
                    "DU" => "Нажать",
                    "U" => "Отпустить",
                    "" => "Ничего",
                    _ => throw new ErrorException("Аргумент macroElement содержал некорректный InputFlags: " + element.InputFlags)
                };
            }
            else
            {
                throw new ErrorException("Аргумент macroElement не был типа KeyboardEventMacroElement!");
            }
        }
    }
}
