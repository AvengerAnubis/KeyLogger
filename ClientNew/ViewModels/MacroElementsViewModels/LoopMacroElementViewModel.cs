using SharpMacroPlayer.ClientNew.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class LoopMacroElementViewModel : MacroElementViewModel
    {
        [ObservableProperty]
        [Range(typeof(long), "1", "9223372036854775807")]
        private string _condition = string.Empty;

        public override MacroElement GetMacroElement()
        {
            ValidateAllProperties();
            if (HasErrors)
                throw new ValidationException("Значение Condition должно быть положительным числом больше 1!");
            return new LoopMacroElement(long.Parse(Condition));
        }

        public override void SetMacroElement(MacroElement macroElement)
        {
            if (macroElement is LoopMacroElement element)
            {
                Condition = element.Condition.ToString();
            }
            else
            {
                throw new ErrorException("Аргумент macroElement не был типа LoopMacroElement!");
            }
        }
    }
}
