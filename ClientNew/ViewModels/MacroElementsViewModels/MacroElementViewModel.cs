using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public abstract partial class MacroElementViewModel : ObservableValidator
    {
        public MacroType MacroType { get; protected set; }

        public MacroElement MacroElement { get => GetMacroElement(); }

        public abstract MacroElement GetMacroElement();

        public abstract void SetMacroElement(MacroElement macroElement);

        public static MacroElementViewModel GetViewModelForMacroElement(MacroElement macroElement)
        {
            MacroElementViewModel viewModel = macroElement.MacroType switch
            {
                MacroType.KEYBOARD_EVENT => new KeyboardEventMacroElementViewModel(),
                MacroType.MOUSE_EVENT => new MouseEventMacroElementViewModel(),
                MacroType.WAITTIME => new WaitTimeMacroElementViewModel(),
                MacroType.LOOP => new LoopMacroElementViewModel(),
                MacroType.END_OF_LOOP => new EndOfLoopMacroElementViewModel(),
                _ => throw new ErrorException($"Не удалось создать ViewModel для макроса типа {Enum.GetName(macroElement.MacroType)}")
            };
            viewModel.SetMacroElement(macroElement);
            return viewModel;
        }
    }
}
