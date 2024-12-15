using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMacroPlayer.ClientNew.ViewModels.MacroElementsViewModels
{
    public sealed partial class VkCodeViewModel : ObservableValidator
    {
        public string[] VkCodes { get => Enum.GetNames<VK>(); }

        [ObservableProperty]
        [EnumDataType(typeof(VK))]
        private string _vkCode = "VK_W";
    }
}
