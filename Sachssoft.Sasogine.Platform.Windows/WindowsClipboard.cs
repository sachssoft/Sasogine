using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Platform.Windows
{
    public class WindowsClipboard : IClipboardService
    {
        private string? _lastStoredText; // nur für einfache AOT-Implementierung

        public void Clear()
        {
                WindowsClear();

            _lastStoredText = null;
        }

        public bool ContainsData(string format)
        {
            return _lastStoredText != null;
        }

        public object? GetData(string format)
        {
            if (_lastStoredText != null)
                return _lastStoredText;

                return WindowsGetData();
        }

        public void SetData(string format, object? data)
        {
            string? text = null;
            if (data is string s)
                text = s;
            else if (data is byte[] bytes)
                text = Convert.ToBase64String(bytes);
            else if (data != null)
                throw new NotSupportedException("Only string or byte[] are supported in AOT mode.");

                WindowsSetData(text);

            _lastStoredText = text;
        }

        #region Windows Native

        private const uint CF_UNICODETEXT = 13;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private void WindowsClear()
        {
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            CloseClipboard();
        }

        private object? WindowsGetData()
        {
            OpenClipboard(IntPtr.Zero);
            try
            {
                IntPtr handle = GetClipboardData(CF_UNICODETEXT);
                if (handle == IntPtr.Zero) return null;
                string text = Marshal.PtrToStringUni(GlobalLock(handle)) ?? "";
                GlobalUnlock(handle);
                return text;
            }
            finally
            {
                CloseClipboard();
            }
        }

        private void WindowsSetData(string? text)
        {
            if (text == null) return;

            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);
            SetClipboardData(CF_UNICODETEXT, hGlobal);
            CloseClipboard();
        }

        #endregion

    }
}
