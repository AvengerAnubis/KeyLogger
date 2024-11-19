using System.Runtime.InteropServices;
using System;

namespace KeyLogger.Utils
{
    public struct INPUT
    {
        public UInt32 Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public Int32 X;
        public Int32 Y;
        public UInt32 MouseData;
        public UInt32 Flags;
        public UInt32 Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public UInt16 VkCode;
        public UInt16 ScanCode;
        public UInt32 Flags;
        public UInt32 Time;
        public IntPtr ExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public UInt32 Msg;
        public UInt16 ParamL;
        public UInt16 ParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public Int32 X;
        public Int32 Y;
        public UInt32 MouseData;
        public UInt32 Flags;
        public UInt32 Time;
        public UIntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public UInt32 VkCode;
        public UInt32 ScanCode;
        public UInt32 Flags;
        public UInt32 Time;
        public UIntPtr DwExtraInfo;
    }
}