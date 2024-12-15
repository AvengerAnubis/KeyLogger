using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.Validators
{
    public class ScreenHeightAttribute : RangeAttribute
    {
        public ScreenHeightAttribute() : base(0, (int)SystemParameters.PrimaryScreenHeight) { }
    }
}
