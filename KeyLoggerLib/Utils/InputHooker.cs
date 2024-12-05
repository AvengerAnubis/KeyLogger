using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static SharpMacroPlayer.Utils.Constants;
using static SharpMacroPlayer.Utils.WinAPIFunctions;

namespace SharpMacroPlayer.Utils
{
    public struct HookCallbackEventArgs
    {
        public IntPtr wParam;
        public IntPtr lParam;

        public HookCallbackEventArgs(IntPtr wParam, IntPtr lParam)
        {
            this.wParam = wParam;
            this.lParam = lParam;
        }
    }
    public class InputHooker : IDisposable
    {
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;

        private CallBackProc _keyboardHookCallback;
        private CallBackProc _mouseHookCallback;

        public event EventHandler<HookCallbackEventArgs> KeyInput;
        public event EventHandler<HookCallbackEventArgs> MouseInput;

        public InputHooker()
        {
            _keyboardHookCallback = KeyboardHookCallback;
            SetHook(_keyboardHookCallback, (int)WH.KEYBOARD_LL);

            _mouseHookCallback = MouseHookCallback;
            SetHook(_mouseHookCallback, (int)WH.MOUSE_LL);
        }

        void IDisposable.Dispose()
        {
            UnhookWindowsHookEx(_keyboardHookID);
            UnhookWindowsHookEx(_mouseHookID);
        }

        private void SetHook(CallBackProc proc, int wh_type)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _keyboardHookID = SetWindowsHookEx(wh_type, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyInput?.Invoke(this, new HookCallbackEventArgs(wParam, lParam));
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseInput?.Invoke(this, new HookCallbackEventArgs(wParam, lParam));
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }
    }

}