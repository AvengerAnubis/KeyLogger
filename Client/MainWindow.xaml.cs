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
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Bindings;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

using Binding = SharpMacroPlayer.Bindings.Binding;
using SharpMacroPlayer.UserControls;
using SharpMacroPlayer.Classes;
using SharpMacroPlayer.Pages;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SharpMacroPlayer
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public KeyboardButton PrevPlayStopButton;
		private BindingsPlayer _bindingsPlayer;

		public void PlayStop(object sender, RoutedEventArgs e)
		{
			if (!_bindingsPlayer.IsRunning)
			{
				_bindingsPlayer.Bindings = _bindings;
				_bindingsPlayer.Start();
				(sender as Button).Content = "Остановка макросов";
			}
			else
			{
				(sender as Button).Content = "Запуск макросов";
				_bindingsPlayer.Stop();
			}
		}
		public void StartStopRecording(object sender, RoutedEventArgs e)
		{
			if (!_recorder.IsRecording)
			{
				if (!string.IsNullOrEmpty(RecordingMacroFilename))
				{
					bool regex = Regex.IsMatch(RecordingMacroFilename, @"^[\w,\s-\p{IsCyrillic}]+$");
					if (regex)
					{
						_fileRecordTo = RecordingMacroFilename;

						(sender as Button).Content = "Остановить запись";
						uint delay = 32, interval = 250;
						uint.TryParse(DefaultDelay, out delay);
						uint.TryParse(MouseMovementRecordingInterval, out interval);
						_recorder.BeginRecording(new MacroRecorderOptions()
						{
							DefaultDelay = delay,
							MouseMovementRecordingInterval = interval,
							SaveDelayBetweenActions = SaveDelayBetweenActions,
							RecordIntermediateMouseMovement = RecordIntermediateMouseMovement
						});
					}
				}
			}
			else
			{
				(sender as Button).Content = "Начать запись";
				_recorder.EndRecording();
				MacroLoaderSaver.SaveMacros(_recorder.Macro, _fileRecordTo + ".json");
				UpdateMacrosFilesArray();

				_recorder.Macro.MacroElements.Clear();
			}
		}

		public bool SaveDelayBetweenActions { get; set; }
		public bool RecordIntermediateMouseMovement { get; set; }
		public string MouseMovementRecordingInterval { get; set; }
		public string DefaultDelay { get; set; }

		public string RecordingMacroFilename { get; set; }
		private string _fileRecordTo;


		public void BindMacros(object sender, string file)
		{
			KeyboardButton button = sender as KeyboardButton;
			_bindings.Bindings.Add(new Binding(button.KeyCode, file));
		}
		public void UnbindMacros(object sender, string file)
		{
			KeyboardButton button = sender as KeyboardButton;
			_bindings.Bindings.RemoveAt(_bindings.Bindings.FindIndex((b) => b.VkCode == button.KeyCode && b.MacroPath == file));
		}
		//public void SelectAsPlayStopButton(object sender)
		//{
		//	if (sender is KeyboardButton)
		//	{
		//		KeyboardButton keyboardButton = sender as KeyboardButton;
		//		PrevPlayStopButton.Click -= PlayStop;
		//		PrevPlayStopButton = keyboardButton;
		//		PrevPlayStopButton.Click += PlayStop;
		//  }
		//}


		public static MainWindow Instance { get; private set; }
		bool wasRecorded = false;

		private MacrosEditorPage _macrosEditorPage;

		private KeyboardButton _playstopButton;

		private MacroRecorder _recorder;
		private InputHooker _interceptKeys;
		public ref InputHooker InterceptKeys {get => ref _interceptKeys; }

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

		private string _selectedBinding;
		private bool _doSave = true;
		public string SelectedBinding
		{
			get => _selectedBinding;
			set
			{
				if (value != null)
				{
					if (_selectedBinding != null && _doSave)
						BindingLoaderSaver.SaveBindings(_bindings, _selectedBinding);
					_selectedBinding = value;
					_bindings = BindingLoaderSaver.LoadBindings(_selectedBinding);
					foreach (KeyboardButton button in keyboardGrid.Children)
					{
					    button.AllBindedMacroses.Clear();
					    foreach (Binding binding in _bindings.Bindings)
					    {
					        if (binding.VkCode == button.KeyCode)
					        {
					            button.AllBindedMacroses.Add(binding.MacroPath);
					        }
					    }
					    button.UpdateBinds();
					}
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedBinding"));
				}
			}

        }

		public string NewBindingFile { get; set; }


        private BindingContainer _bindings;
		public string CurrentBindingFile
		{
			get; private set;
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

		public RelayCommand ReturnToMainPage
		{
			get => new RelayCommand(obj =>
			{
				if (_bindingsPlayer.IsRunning)
				{
					PlayStop(null, null);
				}
				mainFrame.GoBack();
			});
		}
        public RelayCommand NavigateToMacrosEditor
        {
            get => new RelayCommand(obj =>
            {
				MacrosEditorPage page = new MacrosEditorPage() { DataContext = this };
                page.MacrosFilesUpdated += (sender, e) => UpdateMacrosFilesArray();
                mainFrame.Navigate(page);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
		{
			InitializeComponent();
			Closing += (o, e) =>
			{
				BindingLoaderSaver.SaveBindings(_bindings, SelectedBinding);
			};
			Instance = this;

            this.DataContext = this;
            _interceptKeys = new InputHooker();

            _interceptKeys.KeyInput += _interceptKeys_KeyInput;
			_bindings = new BindingContainer();

			foreach (KeyboardButton button in keyboardGrid.Children) 
			{
				button.InputHooker = _interceptKeys;
				button.DataContext = this;
				button.PressedColor = Colors.GreenYellow;
                button.UnpressedColor = Colors.Gray;
				button.BindedColor = Colors.Aqua;
				button.Background = new SolidColorBrush(button.UnpressedColor);

                foreach (Binding binding in _bindings.Bindings)
				{
					if (binding.VkCode == button.KeyCode)
					{
						button.AllBindedMacroses.Add(binding.MacroPath);
                        button.UpdateBinds();
                    }
				}
            }
            foreach (MouseKeyButton button in mouseGrid.Children)
            {
                button.InputHooker = _interceptKeys;
                button.DataContext = this;
                button.PressedColor = Colors.GreenYellow;
                button.UnpressedColor = Colors.Gray;
				button.Background = new SolidColorBrush(button.UnpressedColor);
            }


            _allMacrosFiles = MacroLoaderSaver.GetAllMacros();

			_allBindingsFiles = BindingLoaderSaver.GetAllBindings();
			if (_allBindingsFiles.Length == 0)
			{
				BindingLoaderSaver.SaveBindings(new BindingContainer(), "default.json");
                _allBindingsFiles = BindingLoaderSaver.GetAllBindings();
            }
			SelectedBinding = _allBindingsFiles[0];

			_bindingsPlayer = new BindingsPlayer(ref _interceptKeys);
			_recorder = new MacroRecorder(ref _interceptKeys);

            
            mainFrame.Navigate(new MainPage());
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
                    else
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

		public void CreateBindingProfile()
		{
            bool regex = Regex.IsMatch(NewBindingFile, @"^[\w,\s-\p{IsCyrillic}]+$");
            if (regex)
            {
				BindingLoaderSaver.SaveBindings(new BindingContainer(), NewBindingFile + ".json");
				AllBindingsFiles = BindingLoaderSaver.GetAllBindings();
            }
        }
		public void DeleteBindingProfile()
		{
			_doSave = false;
            BindingLoaderSaver.DeleteBinding(SelectedBinding);
            AllBindingsFiles = BindingLoaderSaver.GetAllBindings();
            if (AllBindingsFiles.Length == 0)
            {
                BindingLoaderSaver.SaveBindings(new BindingContainer(), "default.json");
                AllBindingsFiles = BindingLoaderSaver.GetAllBindings();
            }
            SelectedBinding = AllBindingsFiles[0];
			_doSave = true;
        }

		private void UpdateMacrosFilesArray() => AllMacrosFiles = MacroLoaderSaver.GetAllMacros();

        internal void ChangePressCond(KeyboardButton keyboardButton, object cond)
        {
            foreach (Binding binding in _bindings.Bindings.Where(b => b.VkCode == keyboardButton.KeyCode))
			{
				binding.PlayCondition = (BindingPlayCondition)Enum.Parse(typeof(BindingPlayCondition), cond.ToString());
			}
        }
    }
}