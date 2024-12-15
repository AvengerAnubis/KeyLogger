using SharpMacroPlayer.ClientNew.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class WaitTimeMacroElementViewModel : MacroElementViewModel
    {
        [ObservableProperty]
        [Range(typeof(long), "0", "9223372036854775807")]
        private string _time = string.Empty;

        public override MacroElement GetMacroElement()
        {
            ValidateAllProperties();
            if (HasErrors)
                throw new ValidationException("Значение Time должно быть положительным числом!");
            return new WaitTimeMacroElement(long.Parse(Time));
        }

        public override void SetMacroElement(MacroElement macroElement)
        {
            if (macroElement is WaitTimeMacroElement element)
            {
                Time = element.MsToWait.ToString();
            }
            else
            {
                throw new ErrorException("Аргумент macroElement не был типа WaitTimeMacroElement!");
            }
        }
    }
}
