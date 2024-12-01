using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

#region юзинги для библиотеки
using KeyLogger.Utils;
using KeyLogger.Macros;
#endregion

#region статичные юзинги
using static KeyLogger.Utils.WinAPIFunctions;
using static KeyLogger.Utils.Constants;
#endregion

namespace KeyLogger.Bindings
{
	public class BindingsPlayer : IDisposable
	{
		private bool _isRunning;
		public bool IsRunning
		{
			get => _isRunning;
		}

		private BindingContainer _bindings;
		public ref BindingContainer Bindings
		{
			get => ref _bindings;
		}

		private InputHooker _hooker;

		public BindingsPlayer(ref InputHooker hooker)
		{
			_hooker = hooker;
			_bindings = new BindingContainer();
		}

		public void Start()
		{
			if (!_isRunning)
			{
				_isRunning = true;
				_hooker.KeyInput += OnKeyboardInputGiven;
				_hooker.MouseInput += OnMouseInputGiven;
			}
		}

		public void Stop()
		{
			if (_isRunning)
			{
				_hooker.KeyInput -= OnKeyboardInputGiven;
				_hooker.MouseInput -= OnMouseInputGiven;
			}
		}


		private void OnMouseInputGiven(object sender, HookCallbackEventArgs args)
		{
			// Сразу исключаем движение мыши (на него нельзя забиндить)
			if ((WM)args.wParam != WM.MOUSEMOVE)
			{
				// Получаем структуру из памяти по указателю
				MSLLHOOKSTRUCT data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(args.lParam);

				bool isKeyDown;
				MouseButton mouseButton;

				switch ((WM)args.wParam)
				{
					case WM.LBUTTONDOWN:
						isKeyDown = true;
						mouseButton = MouseButton.LMB;
						break;
					case WM.LBUTTONUP:
						isKeyDown = false;
						mouseButton = MouseButton.LMB;
						break;
					case WM.RBUTTONDOWN:
						isKeyDown = true;
						mouseButton = MouseButton.RMB;
						break;
					case WM.RBUTTONUP:
						isKeyDown = false;
						mouseButton = MouseButton.RMB;
						break;
					case WM.XBUTTONDOWN:
						isKeyDown = true;
						mouseButton = MouseButton.XMB;
						break;
					case WM.XBUTTONUP:
						isKeyDown = false;
						mouseButton = MouseButton.XMB;
						break;
					default:
						return;
				}
				// Проверяем каждый бинд на соответствие нажатой клавиши, выполняем макрос при необходимости
				// (выполняем действия параллейно)
				_bindings.Bindings.AsParallel().ForAll((binding) =>
				{
					binding.ExecuteIfMouseButtonMatches(mouseButton, isKeyDown);
				});
			}
		}

		private void OnKeyboardInputGiven(object sender, HookCallbackEventArgs args)
		{
			// Получаем структуру из памяти по указателю
			KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);

			bool isKeyDown;
			switch ((WM)args.wParam)
			{
				case WM.KEYDOWN:
					isKeyDown = true;
					break;
				case WM.KEYUP:
					isKeyDown = false;
					break;
				case WM.SYSKEYDOWN:
					isKeyDown = true;
					break;
				case WM.SYSKEYUP:
					isKeyDown = false;
					break;
				default:
					return;
			}

			_bindings.Bindings.AsParallel().ForAll((binding) =>
			{
				binding.ExecuteIfVkMatches((VK)data.VkCode, isKeyDown);
			});
		}

		public void Dispose()
		{
			Stop();
		}

		public void WriteToJson(ref Utf8JsonWriter writer)
		{
            writer.WriteStartArray();
            foreach (Binding macroElement in _bindings.Bindings)
            {
                macroElement.WriteToJson(ref writer);
            }
            writer.WriteEndArray();
        }
	}
}
