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
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Bindings;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
using System.Text.RegularExpressions;
using SharpMacroPlayer.Classes;
using System.ComponentModel;
#endregion

namespace SharpMacroPlayer.Pages.MacroElementEditorPages
{
    /// <summary>
    /// Логика взаимодействия для KeyboardElementEditor.xaml
    /// </summary>
    public partial class KeyboardElementEditor : Page, INotifyPropertyChanged
    {
        public class SelectableString
        {
            public string Value { get; set; }
        }

        public event EventHandler<MacroElement> OkPressed;

        private List<SelectableString> _keys;
        public List<SelectableString> Keys
        {
            get => _keys;
            set
            {
                _keys = value;
                keysListBox.ItemsSource = null;
                keysListBox.ItemsSource = Keys;
            }
        }

        public string[] VkCodes
        {
            get => Enum.GetNames(typeof(VK));
        }

        private bool _doKeyDown;
        public bool DoKeyDown
        {
            get => _doKeyDown;
            set
            {
                _doKeyDown = value;
                if (!_doKeyDown && !_doKeyUp)
                {
                    DoKeyUp = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DoKeyDown"));
            }
        }
        private bool _doKeyUp;
        public bool DoKeyUp
        {
            get => _doKeyUp;
            set
            {
                _doKeyUp = value;
                if (!_doKeyUp && !_doKeyDown)
                {
                    DoKeyDown = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DoKeyUp"));
            }
        }


        public RelayCommand AddKey
        {
            get => new RelayCommand(obj =>
            {
                _keys.Add(new SelectableString() { Value = "VK_W" });
                keysListBox.ItemsSource = null;
                keysListBox.ItemsSource = Keys;
            });
        }

        public RelayCommand DelKey
        {
            get => new RelayCommand(obj =>
            {
                if (_keys.Count > 1)
                {
                    _keys.RemoveAt(_keys.Count - 1);
                    keysListBox.ItemsSource = null;
                    keysListBox.ItemsSource = Keys;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public KeyboardElementEditor(MacroElement el)
        {
            InitializeComponent();
            this.DataContext = this;
            _keys = new List<SelectableString>();
            if (el is KeyboardEventMacroElement)
            {
                KeyboardEventMacroElement macroElement = el as KeyboardEventMacroElement;
                foreach (ushort vk in macroElement.KeyCodes)
                {
                    _keys.Add(new SelectableString() { Value = Enum.GetName(typeof(VK), vk) });
                }
                DoKeyDown = macroElement.DoKeyDown;
                DoKeyUp = macroElement.DoKeyUp;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<KeyInputData> keys = new List<KeyInputData>();
            foreach (SelectableString item in keysListBox.Items)
            {
                keys.Add(new KeyInputData() { KeyCode = (ushort)Enum.Parse(typeof(VK), item.Value), KeyEventType = KeyEventType.VIRTUAL_KEY});
            }
            MacroElement macroElement = new KeyboardEventMacroElement(keys.ToArray(), DoKeyDown, DoKeyUp);
            OkPressed?.Invoke(this, macroElement);
        }
    }
}
