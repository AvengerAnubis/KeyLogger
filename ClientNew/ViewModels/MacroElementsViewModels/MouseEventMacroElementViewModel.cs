using SharpMacroPlayer.ClientNew.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class MouseEventMacroElementViewModel : MacroElementViewModel
    {
        [ObservableProperty]
        [EnumDataType(typeof(MouseButton))]
        private string _buttonPressed = string.Empty;
        public string[] MouseButtons { get => Enum.GetNames<MouseButton>(); }

        [ObservableProperty]
        private bool _doMove = false;

        [ObservableProperty]
        private bool _isAbsolutePositioning = false;

        [ObservableProperty]
        [ScreenWidth]
        private string _x = string.Empty;

        [ObservableProperty]
        [ScreenHeight]
        private string _y = string.Empty;

        [ObservableProperty]
        [Range(-2147483648, 2147483647)]
        private string _mouseWheelMove = string.Empty;

        [ObservableProperty]
        private string _keyState = string.Empty;
        public string[] KeyStates
        { 
            get =>
            [
                "Нажать", "Удержать", "Отпустить", "Ничего"
            ];
        }

        [ObservableProperty]
        private string _xmbButton = string.Empty;
        public string[] XmbButtons
        {
            get =>
            [
                "Нет", "Кнопка 4", "Кнопка 5"
            ];
        }


        public override MacroElement GetMacroElement()
        {
            ValidateAllProperties();
            if (HasErrors)
                throw new ValidationException("Неверные значения!");
            int xmbButton = XmbButton switch
            {
                "Нет" => 0,
                "Кнопка 4" => 1,
                "Кнопка 5" => 2,
                _ => throw new ErrorException("Ничего не использовалось")
            };
            bool[] keyStates = KeyState switch
            {
                "Нажать" => [true, true],
                "Удержать" => [true, false],
                "Отпустить" => [false, true],
                "Ничего" => [false, false],
                _ => throw new ErrorException("Ничего не использовалось")

            };
            return new MouseEventMacroElement(
                keyStates[0], keyStates[1],
                Enum.Parse<MouseButton>(ButtonPressed),
                DoMove, IsAbsolutePositioning,
                int.Parse(X), int.Parse(Y),
                xmbButton, int.Parse(MouseWheelMove)
                );
        }

        public override void SetMacroElement(MacroElement macroElement)
        {
            if (macroElement is MouseEventMacroElement element)
            {
                ButtonPressed = element.ButtonPressed.ToString();
                DoMove = element.DoMove;
                IsAbsolutePositioning = element.IsAbsolutePositioning;
                X = element.X.ToString();
                Y = element.Y.ToString();
                MouseWheelMove = element.MouseWheelMove.ToString();
                KeyState = element.InputFlags switch
                {
                    "D" => "Удержать",
                    "DU" => "Нажать",
                    "U" => "Отпустить",
                    "" => "Ничего",
                    _ => throw new ErrorException("Аргумент macroElement содержал некорректный InputFlags: " + element.InputFlags)
                };
                XmbButton = element.XmbButtonNumber switch
                {
                    0 => "Нет",
                    1 => "Кнопка 4",
                    2 => "Кнопка 5",
                    _ => throw new ErrorException("Аргумент macroElement содержал некорректный XmbButtonNumber: " + element.XmbButtonNumber.ToString())
                };
            }
            else
            {
                throw new ErrorException("Аргумент macroElement не был типа MouseEventMacroElement!");
            }
        }
    }
}
