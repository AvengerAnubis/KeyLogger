using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.Validators
{
    public class ScreenWidthAttribute : RangeAttribute
    {
        public ScreenWidthAttribute() : base(0, (int)SystemParameters.PrimaryScreenWidth) { }
    }
}
