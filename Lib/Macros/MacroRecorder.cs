using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#region юзинги для библиотеки
using SharpMacroPlayer.Utils;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Macros.MacroElements;
#endregion

#region статичные юзинги
using static SharpMacroPlayer.Utils.WinAPIFunctions;
using static SharpMacroPlayer.Utils.Constants;
#endregion

namespace SharpMacroPlayer.Macros
{
    /// <summary>
    /// Структура с параметрами записи макросов
    /// </summary>
    public struct MacroRecorderOptions
    {
        /// <summary>
        /// Записывать промежуточное движение мыши (движение мыши между кликами)?
        /// </summary>
        public bool RecordIntermediateMouseMovement;
        /// <summary>
        /// Интервал записи движения мыши (только при <see cref="RecordIntermediateMouseMovement">RecordIntermediateMouseMovement</see> = true)
        /// </summary>
        public uint MouseMovementRecordingInterval;
        /// <summary>
        /// Сохранять промежутки между действиями?
        /// </summary>
        public bool SaveDelayBetweenActions;
        /// <summary>
        /// Интервал между действиями по-умолчанию (только при <see cref="SaveDelayBetweenActions">SaveDelayBetweenActions</see> = false)
        /// </summary>
        public uint DefaultDelay;
    }
    public class MacroRecorder : IDisposable
    {
        private MacroRecorderOptions _options;

        private Stopwatch _recordStopwatch;

        private InputHooker _hooker;
        public InputHooker Hooker
        {
            get => _hooker;
        }

        private bool _isRecording = false;
        public bool IsRecording
        {
            get => _isRecording;
        }

        private Macro _macro;
        public Macro Macro
        {
            get => (!_isRecording) ? _macro : null;
        }

        private long _timeSinceStart;

        public MacroRecorder(ref InputHooker hooker) 
        {
            _hooker = hooker;
            _macro = new Macro();
            _recordStopwatch = new Stopwatch();
        }

        public void BeginRecording(MacroRecorderOptions options)
        {
            if (!_isRecording)
            {
                _options = options;
                _timeSinceStart = 0;

                _macro.MacroElements.Clear();
                _hooker.MouseInput += OnMouseInputGiven;
                _hooker.KeyInput += OnKeyboardInputGiver;

                _isRecording = true;
                _recordStopwatch.Restart();
            }
        }
        public void EndRecording()
        {
            if (_isRecording)
            {
                _hooker.MouseInput -= OnMouseInputGiven;
                _hooker.KeyInput -= OnKeyboardInputGiver;
                _isRecording = false;
            }
        }

        /// <summary>
        /// Метод, обрабатывающий ввод с мыши
        /// </summary>
        /// <param name="args">Данные о вводе с мыши</param>
        private void OnMouseInputGiven(object sender, HookCallbackEventArgs args)
        {
            long elapsed = _recordStopwatch.ElapsedMilliseconds - _timeSinceStart;

            // Получаем структуру из памяти по указателю
            MSLLHOOKSTRUCT data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(args.lParam);

            if ((WM)args.wParam == WM.MOUSEMOVE)
            {
                if (_options.RecordIntermediateMouseMovement)
                {
                    if (elapsed > _options.MouseMovementRecordingInterval)
                    {
                        _timeSinceStart = _recordStopwatch.ElapsedMilliseconds;
                        if (_options.SaveDelayBetweenActions)
                        {
                            // добавляем действие - ожидание
                            _macro.MacroElements.Add(new WaitTimeMacroElement(elapsed));
                        }
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, false, MouseButton.NOTHING, true, true, data.X, data.Y));
                        if (!_options.SaveDelayBetweenActions)
                        {
                            // и если дефолтный интервал больше 0
                            if (_options.DefaultDelay > 0)
                            {
                                // добавляем действие - ожидание
                                _macro.MacroElements.Add(new WaitTimeMacroElement(_options.DefaultDelay));
                            }
                        }
                    }
                }
            }
            else
            {
                _timeSinceStart = _recordStopwatch.ElapsedMilliseconds;
                // Если в настройках указано сохранять интервал между действиями
                if (_options.SaveDelayBetweenActions)
                {
                    // добавляем действие - ожидание
                    _macro.MacroElements.Add(new WaitTimeMacroElement(elapsed));
                }
                // Добавляем действие в зависимости от типа действия пользователя
                switch ((WM)args.wParam)
                {
                    case WM.LBUTTONDOWN:
                        _macro.MacroElements.Add(new MouseEventMacroElement(true, false, MouseButton.LMB, true, true, data.X, data.Y));
                        break;
                    case WM.LBUTTONUP:
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, true, MouseButton.LMB, true, true, data.X, data.Y));
                        break;
                    case WM.MOUSEWHEEL:
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, false, MouseButton.WHEEL, true, true, data.X, data.Y, 0, (int)((short)(data.MouseData >> 16))));
                        break;
                    case WM.MOUSEHWHEEL:
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, false, MouseButton.HWHEEL, true, true, data.X, data.Y, 0, (int)((short)(data.MouseData >> 16))));
                        break;
                    case WM.RBUTTONDOWN:
                        _macro.MacroElements.Add(new MouseEventMacroElement(true, false, MouseButton.RMB, true, true, data.X, data.Y));
                        break;
                    case WM.RBUTTONUP:
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, true, MouseButton.RMB, true, true, data.X, data.Y));
                        break;
                    case WM.XBUTTONDOWN:
                        _macro.MacroElements.Add(new MouseEventMacroElement(true, false, MouseButton.XMB, true, true, data.X, data.Y, (int)data.MouseData));
                        break;
                    case WM.XBUTTONUP:
                        _macro.MacroElements.Add(new MouseEventMacroElement(false, true, MouseButton.XMB, true, true, data.X, data.Y, (int)data.MouseData));
                        break;
                }
                // Если в настройках указано НЕ сохранять интервал между действиями
                if (!_options.SaveDelayBetweenActions)
                {
                    // и если дефолтный интервал больше 0
                    if (_options.DefaultDelay > 0)
                    {
                        // добавляем действие - ожидание
                        _macro.MacroElements.Add(new WaitTimeMacroElement(_options.DefaultDelay));
                    }
                }
            }
            
        }
        /// <summary>
        /// Метод, обрабатыващий ввод с клавиатуры
        /// </summary>
        /// <param name="args">Данные о вводе с клавиатуры</param>
        private void OnKeyboardInputGiver(object sender, HookCallbackEventArgs args)
        {
            long elapsed = _recordStopwatch.ElapsedMilliseconds - _timeSinceStart;
            _timeSinceStart = _recordStopwatch.ElapsedMilliseconds;
            // Получаем структуру из памяти по указателю
            KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);
            // Если в настройках указано сохранять интервал между действиями
            if (_options.SaveDelayBetweenActions)
            {
                // добавляем действие - ожидание
                _macro.MacroElements.Add(new WaitTimeMacroElement(elapsed));
            }
            bool keyDown = false;
            if ((WM)args.wParam == WM.KEYDOWN || (WM)args.wParam == WM.SYSKEYDOWN)
                keyDown = true;

            // Добавляем действие
            _macro.MacroElements.Add(new KeyboardEventMacroElement(new KeyInputData[] { new KeyInputData() { KeyCode = (ushort)data.VkCode, KeyEventType = KeyEventType.VIRTUAL_KEY } }, keyDown, !keyDown));

            // Если в настройках указано НЕ сохранять интервал между действиями
            if (!_options.SaveDelayBetweenActions)
            {
                // и если дефолтный интервал больше 0
                if (_options.DefaultDelay > 0)
                {
                    // добавляем действие - ожидание
                    _macro.MacroElements.Add(new WaitTimeMacroElement(_options.DefaultDelay));
                }
            }
        }

        public void Dispose()
        {
            if (_isRecording)
            {
                EndRecording();
            }
        }
    }


    [Serializable]
    public class MacroRecorderException : Exception
    {
        public MacroRecorderException() { }
        public MacroRecorderException(string message) : base(message) { }
        public MacroRecorderException(string message, Exception inner) : base(message, inner) { }
        protected MacroRecorderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
