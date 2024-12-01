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
#endregion

namespace KeyLogger.Pages
{
    /// <summary>
    /// Логика взаимодействия для MacrosEditorPage.xaml
    /// </summary>
    public partial class MacrosEditorPage : Page
    {
        public static MacrosEditorPage Instanse {  get; set; }

        public event EventHandler MacrosFilesUpdated;  

        private Macro _selectedMacro;

        public RelayCommand DeleteMacro = new RelayCommand(macro => 
        {
            MacroLoaderSaver.DeleteMacro(macro.ToString());
            Instanse.MacrosFilesUpdated?.Invoke(null, EventArgs.Empty);
        });

        public RelayCommand EditMacro = new RelayCommand(macro => Instanse.SetMacroToEditor(macro.ToString()));

        public MacrosEditorPage()
        {
            InitializeComponent();
            Instanse = this;
        }

        public void SetMacroToEditor(string macroFile)
        {
            Macro macro = MacroLoaderSaver.LoadMacro(macroFile);
            SetMacroToEditor(macro);
        }
        public void SetMacroToEditor(Macro macro)
        {
            _selectedMacro = macro;
            macroElementsListBox.ItemsSource = _selectedMacro.MacroElements;
        }

        private void AddMacro_Click(object sender, RoutedEventArgs e)
        {
            string newMacroName = newMacroNameTextBox.Text;
            bool regex = Regex.IsMatch(newMacroName, @"^[\w,\s-\p{IsCyrillic}]+$");
            if (regex)
            {
                Macro macro = new Macro();
                SetMacroToEditor(_selectedMacro);
                MacroLoaderSaver.SaveMacros(_selectedMacro, newMacroName);
                MacrosFilesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
