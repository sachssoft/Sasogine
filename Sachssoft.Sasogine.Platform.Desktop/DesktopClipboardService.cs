using Sachssoft.Sasogine.Services;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Sachssoft.Sasogine.Platform
{
    public class DesktopClipboardService : IClipboardService
    {
        private string? _lastStoredText; // nur für einfache AOT-Implementierung

        public void Clear()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsClear();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                LinuxClear();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                MacClear();

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsGetData();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LinuxGetData();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return MacGetData();

            return null;
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsSetData(text);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                LinuxSetData(text);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                MacSetData(text);

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

        #region Linux

        private void LinuxClear() => RunProcess("xclip", "-selection clipboard -i /dev/null");

        private object? LinuxGetData() => RunProcessOutput("xclip", "-selection clipboard -o");

        private void LinuxSetData(string? text)
        {
            if (text == null) return;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard",
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };
            proc.Start();
            proc.StandardInput.Write(text);
            proc.StandardInput.Close();
        }

        #endregion

        #region macOS

        private void MacClear() => RunProcess("pbcopy", "");

        private object? MacGetData() => RunProcessOutput("pbpaste", "");

        private void MacSetData(string? text)
        {
            if (text == null) return;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pbcopy",
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };
            proc.Start();
            proc.StandardInput.Write(text);
            proc.StandardInput.Close();
        }

        #endregion

        #region Helpers

        private static void RunProcess(string fileName, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                }
            };
            proc.Start();
            proc.WaitForExit();
        }

        private static string RunProcessOutput(string fileName, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return output;
        }

        #endregion
    }
}
