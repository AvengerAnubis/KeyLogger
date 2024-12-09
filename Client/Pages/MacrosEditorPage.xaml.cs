using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.ComponentModel;

using SharpMacroPlayer.Classes;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Macros.MacroElements;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion


namespace SharpMacroPlayer.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacrosEditorPage.xaml
    /// </summary>
    public partial class MacrosEditorPage : Page, INotifyPropertyChanged
    {
        // Статичный экземпляр (для Command'ов)
        //public static MacrosEditorPage Instanse {  get; set; }

        public event EventHandler MacrosFilesUpdated;

        private Macro _selectedMacro;
        private string _selectedMacroFilepath;
        public string SelectedMacroFilepath
        {
            get => _selectedMacroFilepath;
            set
            {
                _selectedMacroFilepath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedMacroFilepath"));
            }
        }

        public List<MacroElement> MacroElements
        {
            get => (_selectedMacro != null) ? _selectedMacro.MacroElements : new List<MacroElement>();
        }

        public RelayCommand DeleteMacro 
        { 
            get => new RelayCommand(macro =>
            {
                MacroLoaderSaver.DeleteMacro(macro.ToString());
                MacrosFilesUpdated?.Invoke(null, EventArgs.Empty);
            });
        }
        public RelayCommand EditMacro
        {
            get => new RelayCommand(macro => SetMacroToEditor(macro.ToString()));
        }

        public RelayCommand DeleteMacroElement
        {
            get => new RelayCommand(el =>
            {
                _selectedMacro.MacroElements.Remove(el as MacroElement);
                SetMacroToEditor();
            });
        }
        public RelayCommand MoveDownMacroElement
        {
            get => new RelayCommand(el =>
            {
                int index = _selectedMacro.MacroElements.IndexOf(el as MacroElement);
                if (index < _selectedMacro.MacroElements.Count - 1)
                {
                    _selectedMacro.MacroElements.Reverse(index, 2);
                    SetMacroToEditor();
                }
            });
        }
        public RelayCommand MoveUpMacroElement
        {
            get => new RelayCommand(el =>
            {
                int index = _selectedMacro.MacroElements.IndexOf(el as MacroElement);
                if (index > 0)
                {
                    _selectedMacro.MacroElements.Reverse(index - 1, 2);
                    SetMacroToEditor();
                }
            });
        }
        public RelayCommand EditMacroElement
        {
            get => new RelayCommand(el =>
            {
                MacroElementEditorWindow window = new MacroElementEditorWindow(el as MacroElement);
                if (window.IsEditorAwaiable)
                {
                    window.OkPressed += (s, e) =>
                    {
                        _selectedMacro.MacroElements[_selectedMacro.MacroElements.IndexOf(el as MacroElement)] = e;
                        SetMacroToEditor();
                        window.Close();
                    };
                    window.ShowDialog();
                }
            });
        }

        public RelayCommand SaveMacro
        {
            get => new RelayCommand(obj =>
            {
                if (_selectedMacro != null)
                {
                    MacroLoaderSaver.SaveMacros(_selectedMacro, _selectedMacroFilepath);
                }
            });
        }
        public RelayCommand AddMacroElement
        {
            get => new RelayCommand(type =>
            {
                if (_selectedMacro != null)
                {
                    MacroElement macroElement;
                    switch (type)
                    {
                        case "KB":
                            macroElement = new KeyboardEventMacroElement(new ushort[] {(ushort)VK.VK_W}, new KeyEventType[] {KeyEventType.VIRTUAL_KEY}, true, true);
                            break;
                        case "MB":
                            macroElement = new MouseEventMacroElement(true, true, MouseButton.LMB, false, true, 0, 0);
                            break;
                        case "WT":
                            macroElement = new WaitTimeMacroElement(100);
                            break;
                        case "LP":
                            macroElement = new LoopMacroElement(3);
                            break;
                        case "ELP":
                            macroElement = new EndOfLoopMacroElement();
                            break;
                        default: return;
                    }
                    _selectedMacro.MacroElements.Add(macroElement);
                    SetMacroToEditor();
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public MacrosEditorPage()
        {
            InitializeComponent();
            macroElementsListBox.Items.Clear();
            

        }

        public void SetMacroToEditor(string macroFile)
        {
            if (macroFile != null)
            {
                try
                {
                    Macro macro = MacroLoaderSaver.LoadMacro(macroFile);
                    SetMacroToEditor(macro);
                    SelectedMacroFilepath = macroFile;
                }
                catch (MacroLoadException e)
                {
                    MessageBox.Show(e.Message, "Ошибка");
                }
            }
        }
        public void SetMacroToEditor(Macro macro)
        {
            if (macro != null)
            {
                _selectedMacro = macro;
                SetMacroToEditor();
            }
        }
        public void SetMacroToEditor()
        {
            macroElementsListBox.ItemsSource = null;
            macroElementsListBox.ItemsSource = MacroElements;
        }

        private void AddMacro_Click(object sender, RoutedEventArgs e)
        {
            string newMacroName = newMacroNameTextBox.Text;
            bool regex = Regex.IsMatch(newMacroName, @"^[\w,\s-\p{IsCyrillic}]+$");
            if (regex)
            {
                Macro macro = new Macro();
                SetMacroToEditor(macro);
                MacroLoaderSaver.SaveMacros(_selectedMacro, newMacroName + ".json");
                MacrosFilesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
