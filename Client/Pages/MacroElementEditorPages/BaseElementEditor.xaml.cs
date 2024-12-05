using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
using KeyLogger.Bindings;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
using System.Text.RegularExpressions;
using KeyLogger.Classes;
using System.ComponentModel;
#endregion

namespace KeyLogger.Pages.MacroElementEditorPages
{
    /// <summary>
    /// Логика взаимодействия для BaseElementEditor.xaml
    /// </summary>
    public partial class BaseElementEditor : Page
    {
        public event EventHandler<MacroElement> OkPressed;

        public BaseElementEditor()
        {
            InitializeComponent();
        }
    }
}
