using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Runtime.InteropServices;
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
#endregion

using Binding = KeyLogger.Bindings.Binding;
using KeyLogger.UserControls;
using KeyLogger.Classes;
using KeyLogger.Pages;
using System.ComponentModel;

namespace KeyLogger
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		bool wasRecorded = false;

		private MacrosEditorPage _macrosEditorPage;

		private KeyboardButton _playstopButton;

		private MacroRecorder _recorder;
		private InputHooker _interceptKeys;

		private string[] _allBindingsFiles;
		public string[] AllBindingsFiles
		{
			get
			{
				return _allBindingsFiles;
            }
			set
			{
				_allBindingsFiles = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AllBindingsFiles"));
			}
		}


        private BindingContainer _bindings;
		public string CurrentBindingFile
		{
			get; private set;
		}

		public RelayCommand BindMacros
		{
			get; set;
		}

        private string[] _allMacrosFiles;


        public string[] AllMacrosFiles
		{
			get
			{
				return _allMacrosFiles;
			}
			set
			{
				_allMacrosFiles = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AllMacrosFiles"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;
            BindMacros = new RelayCommand((macroPath) =>
			{

			});
            _interceptKeys = new InputHooker();

            _interceptKeys.KeyInput += _interceptKeys_KeyInput;

			foreach (KeyboardButton button in keyboardGrid.Children) 
			{
				button.InputHooker = _interceptKeys;
				button.PressedColor = Colors.Red;
				button.UnpressedColor = Colors.Green;
			}

			_allMacrosFiles = MacroLoaderSaver.GetAllMacros();

			_allBindingsFiles = BindingLoaderSaver.GetAllBindings();

			if (_allBindingsFiles.Length == 0)
			{
                _bindings = new BindingContainer();
                BindingLoaderSaver.SaveBindings(_bindings, "newProfile.json");
				CurrentBindingFile = "newProfile";
            }

            _macrosEditorPage = new MacrosEditorPage() { DataContext = this };
            _macrosEditorPage.MacrosFilesUpdated += (sender, e) => UpdateMacrosFilesArray();
            mainFrame.Navigate(_macrosEditorPage);
        }

		private void UpdateKeyStates()
		{
			foreach (Binding binding in _bindings.Bindings)
			{
                foreach (KeyboardButton button in keyboardGrid.Children)
                {
                    if (button.KeyCode == binding.VkCode)
					{
						button.UnpressedColor = Colors.Blue;
                    }
                    if (button.KeyCode == binding.VkCode)
                    {
                        button.UnpressedColor = Colors.Green;
                    }
					button.Background = new SolidColorBrush(button.UnpressedColor);
                }
            }
		}

        private void _interceptKeys_KeyInput(object sender, HookCallbackEventArgs args)
        {
			// Получаем структуру из памяти по указателю
            KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);

            if (_playstopButton != null)
			{
				if (_playstopButton.KeyCode == (VK)data.VkCode)
				{

				}
			}
        }



		private void UpdateMacrosFilesArray() => _allMacrosFiles = MacroLoaderSaver.GetAllMacros();
    }
}