using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class EndOfLoopMacroElementViewModel : MacroElementViewModel
    {
        public override MacroElement GetMacroElement()
        {
            return new EndOfLoopMacroElement();
        }

        public override void SetMacroElement(MacroElement macroElement) { }
    }
}
