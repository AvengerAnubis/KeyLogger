using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Pages.MacroElementEditorPages;
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
using System.Windows.Shapes;

namespace SharpMacroPlayer
{
    /// <summary>
    /// Логика взаимодействия для MacroElementEditorWindow.xaml
    /// </summary>
    public partial class MacroElementEditorWindow : Window
    {
        public bool IsEditorAwaiable { get; private set; } = true;

        public event EventHandler<MacroElement> OkPressed;
        public MacroElement MacroElementToDisplay
        {
            get; set;
        }

        public MacroElementEditorWindow(MacroElement el)
        {
            InitializeComponent();
            MacroElementToDisplay = el;
            Page page = new Page();
            switch (el.MacroType)
            {
                case MacroType.KEYBOARD_EVENT:
                    KeyboardElementEditor kbEditor = new KeyboardElementEditor(el);
                    kbEditor.OkPressed += (o, a) => OkPressed?.Invoke(o, a);
                    page = kbEditor;
                    break;
                case MacroType.MOUSE_EVENT:
                    MouseElementEditor mEditor = new MouseElementEditor(el);
                    mEditor.OkPressed += (o, a) => OkPressed?.Invoke(o, a);
                    page = mEditor;
                    break;
                case MacroType.WAITTIME:
                    WaitTimeElementEditor wtEditor = new WaitTimeElementEditor(el);
                    wtEditor.OkPressed += (o, a) => OkPressed?.Invoke(o, a);
                    page = wtEditor;
                    break;
                case MacroType.WAIT_TILL_KEY:
                    IsEditorAwaiable = false;
                    break;
                case MacroType.LOOP:
                    LoopElementEditor lEditor = new LoopElementEditor(el);
                    lEditor.OkPressed += (o, a) => OkPressed?.Invoke(o, a);
                    page = lEditor;
                    break;
                case MacroType.END_OF_LOOP:
                    IsEditorAwaiable = false;
                    break;
                default:
                    IsEditorAwaiable = false;
                    break;
            }


            mainFrame.Navigate(page);
        }
        
    }
}
