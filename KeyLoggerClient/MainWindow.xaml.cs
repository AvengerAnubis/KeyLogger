using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

using KeyLogger.Utils;
using KeyLogger.Macros;
using static KeyLogger.Utils.Constants;
using static KeyLogger.Utils.WinAPIFunctions;

namespace KeyLogger
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool wasRecorded = false;

		private MacroRecorder _recorder;
		private InputHooker _interceptKeys;

		public MainWindow()
		{
			InitializeComponent();
			_interceptKeys = new InputHooker();
			_recorder = new MacroRecorder(ref _interceptKeys);
			_interceptKeys.KeyInput += InterceptKeys_KeyInput;
			_interceptKeys.MouseInput += _interceptKeys_MouseInput;
		}

		private void _interceptKeys_MouseInput(object sender, HookCallbackEventArgs e)
		{
			MSLLHOOKSTRUCT keybdInputStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(e.lParam);
			mLabel1.Content = $"Input type = {Enum.GetName(typeof(WM), (Int32)e.wParam)}";
			mLabel2.Content = $"X, Y = {keybdInputStruct.X.ToString()}, {keybdInputStruct.Y.ToString()}";
			mLabel3.Content = $"MouseData = {keybdInputStruct.MouseData.ToString()}";
			mLabel4.Content = $"Flags = {keybdInputStruct.Flags.ToString()}";
			mLabel5.Content = $"Time = {keybdInputStruct.Time.ToString()}";
		}

		private void InterceptKeys_KeyInput(object sender, HookCallbackEventArgs e)
		{
			KBDLLHOOKSTRUCT keybdInputStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(e.lParam);
			kbLabel1.Content = $"Input type = {Enum.GetName(typeof(WM), (Int32)e.wParam)}";
			kbLabel2.Content = $"VkCode = {Enum.GetName(typeof(VK), keybdInputStruct.VkCode)}";
			kbLabel3.Content = $"ScanCode = {keybdInputStruct.ScanCode.ToString()}";
			kbLabel4.Content = $"Flags = {keybdInputStruct.Flags.ToString()}";
			kbLabel5.Content = $"Time = {keybdInputStruct.Time.ToString()}";
		}
		
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			/*
            KeyEventType[] keys = new KeyEventType[8];
			VK[] text = new VK[]
			{
				VK.VK_H, VK.VK_F, VK.VK_OEM_COMMA, VK.VK_J, VK.VK_N, VK.VK_F, VK.VK_T, VK.VK_N
			};
			
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = KeyEventType.VIRTUAL_KEY;

            }
			List<MacroElement> macroes = new List<MacroElement>()
			{
				new MouseEventMacroElement("DU", MouseButton.RMB, true, true, 500, 200, 0, 0),
				new WaitTimeMacroElement(128),
				new MouseEventMacroElement("DU", MouseButton.LMB, true, true, 550, 360, 0, 0),
				new WaitTimeMacroElement(128),
				new MouseEventMacroElement("DU", MouseButton.LMB, true, true, 1000, 360, 0, 0),
				new WaitTimeMacroElement(128),
				new KeyboardEventMacroElement(text.Select((el) => (ushort)el).ToArray(), keys, true, true)
			};
			Macro macro = new Macro(macroes);
			*/
			if (wasRecorded)
			{
				return;
			}
			if (!_recorder.IsRecording)
			{
                _recorder.BeginRecording(new MacroRecorderOptions()
                {
                    SaveDelayBetweenActions = true,
                    MouseMovementRecordingInterval = 25,
                    RecordIntermediateMouseMovement = true
                });
            }
            else
            {
                wasRecorded = true;
                _recorder.EndRecording();
				Task.Run(async () =>
				{
					await Task.Delay(1500);
					MacroPlayer player = new MacroPlayer(_recorder.Macro);
					player.RunMacro();
					await Task.Delay(1000);
					MacroLoaderSaver.SaveMacro(_recorder.Macro, "C:\\Users\\Admen\\Desktop\\macro.json", false);
				});
            }
        }
	}
}