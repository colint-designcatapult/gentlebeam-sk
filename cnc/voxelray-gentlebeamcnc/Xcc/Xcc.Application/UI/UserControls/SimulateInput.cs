using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Xcc.Application.UI.UserControls;

public static class SimulateInput
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

    /// <summary>
    /// Send a key down and hold it down until SendKeyUp method is called
    /// </summary>
    public static void SendKeyDown(params Key[] keys)
    {
        List<INPUT> inputs = [];

        foreach (var key in keys)
        {
            ushort keyCode = (ushort)KeyInterop.VirtualKeyFromKey(key);

            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT { Vk = keyCode, ExtraInfo = IntPtr.Zero };
            inputs.Add(input);
        }

        if (SendInput((uint)inputs.Count(), inputs.ToArray(), Marshal.SizeOf(typeof(INPUT))) == 0)
            throw new Exception();
    }

    /// <summary>
    /// Release a key that is being hold down
    /// </summary>
    public static void SendKeyUp(params Key[] keys)
    {
        List<INPUT> inputs = [];

        foreach (var key in keys.Reverse())
        {
            ushort keyCode = (ushort)KeyInterop.VirtualKeyFromKey(key);

            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT { Vk = keyCode, ExtraInfo = IntPtr.Zero, Flags = 2  };
            inputs.Add(input);
        }

        if (SendInput((uint)inputs.Count(), inputs.ToArray(), Marshal.SizeOf(typeof(INPUT))) == 0)
            throw new Exception();
    }

    /// <summary>
    /// Simulate key press
    /// </summary>
    public static void SendKeyPress(params Key[] keys)
    {
        List<INPUT> inputs = [];

        foreach (var key in keys)
        {
            ushort keyCode = (ushort)KeyInterop.VirtualKeyFromKey(key);

            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT { Vk = keyCode, ExtraInfo = IntPtr.Zero };
            inputs.Add(input);
        }

        foreach (var key in keys.Reverse())
        {
            ushort keyCode = (ushort)KeyInterop.VirtualKeyFromKey(key);

            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT { Vk = keyCode, ExtraInfo = IntPtr.Zero, Flags = 2 };
            inputs.Add(input);
        }

        if (SendInput((uint)inputs.Count(), inputs.ToArray(), Marshal.SizeOf(typeof(INPUT))) == 0)
            throw new Exception();
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        public uint Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    /// <summary>
    /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    /// <summary>
    /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }
}