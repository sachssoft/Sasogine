using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Sachssoft.Sasogine.Surface.Utility.Clipboard;

static class WindowsClipboard
{
    public static void SetText(string text)
    {
        OpenClipboard();

        EmptyClipboard();
        nint hGlobal = nint.Zero;
        try
        {
            var bytes = (text.Length + 1) * 2;
            hGlobal = Marshal.AllocHGlobal(bytes);

            if (hGlobal == nint.Zero)
            {
                ThrowWin32();
            }

            var target = GlobalLock(hGlobal);

            if (target == nint.Zero)
            {
                ThrowWin32();
            }

            try
            {
                Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
            }
            finally
            {
                GlobalUnlock(target);
            }

            if (SetClipboardData(cfUnicodeText, hGlobal) == nint.Zero)
            {
                ThrowWin32();
            }

            hGlobal = nint.Zero;
        }
        finally
        {
            if (hGlobal != nint.Zero)
            {
                Marshal.FreeHGlobal(hGlobal);
            }

            CloseClipboard();
        }
    }

    public static void OpenClipboard()
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(nint.Zero))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            Thread.Sleep(100);
        }
    }

    public static string GetText()
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText))
        {
            return null;
        }

        nint handle = nint.Zero;

        nint pointer = nint.Zero;
        try
        {
            OpenClipboard();
            handle = GetClipboardData(cfUnicodeText);
            if (handle == nint.Zero)
            {
                return null;
            }

            pointer = GlobalLock(handle);
            if (pointer == nint.Zero)
            {
                return null;
            }

            var size = GlobalSize(handle);
            var buff = new byte[size];

            Marshal.Copy(pointer, buff, 0, size);

            return Encoding.Unicode.GetString(buff).TrimEnd('\0');
        }
        finally
        {
            if (pointer != nint.Zero)
            {
                GlobalUnlock(handle);
            }

            CloseClipboard();
        }
    }

    const uint cfUnicodeText = 13;

    static void ThrowWin32()
    {
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("User32.dll", SetLastError = true)]
    static extern nint GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern nint GlobalLock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalUnlock(nint hMem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool OpenClipboard(nint hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    static extern nint SetClipboardData(uint uFormat, nint data);

    [DllImport("user32.dll")]
    static extern bool EmptyClipboard();

    [DllImport("Kernel32.dll", SetLastError = true)]
    static extern int GlobalSize(nint hMem);
}