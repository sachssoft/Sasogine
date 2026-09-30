using Sachssoft.Engine.Services.Platform;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Windows clipboard implementation backed by the native Win32 clipboard API.
/// </summary>
public sealed class WindowsClipboard : IClipboardService
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMoveable = 0x0002;

    private string? _lastStoredText;

    /// <inheritdoc />
    public void Clear()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return;

        try
        {
            EmptyClipboard();
            _lastStoredText = null;
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <inheritdoc />
    public bool ContainsData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        return _lastStoredText is not null || IsClipboardFormatAvailable(CfUnicodeText);
    }

    /// <inheritdoc />
    public object? GetData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (_lastStoredText is not null)
            return _lastStoredText;

        if (!OpenClipboard(IntPtr.Zero))
            return null;

        try
        {
            var handle = GetClipboardData(CfUnicodeText);
            if (handle == IntPtr.Zero)
                return null;

            var pointer = GlobalLock(handle);
            if (pointer == IntPtr.Zero)
                return null;

            try
            {
                return Marshal.PtrToStringUni(pointer);
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <inheritdoc />
    public void SetData(string format, object? data)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (data is null)
        {
            Clear();
            return;
        }

        var text = data switch
        {
            string value => value,
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => throw new NotSupportedException(
                "The Windows clipboard implementation currently supports string and byte[] data only.")
        };

        var bytesToCopy = Encoding.Unicode.GetBytes(text + '\0');
        var memory = GlobalAlloc(GmemMoveable, (UIntPtr)(uint)bytesToCopy.Length);
        if (memory == IntPtr.Zero)
            return;

        var pointer = GlobalLock(memory);
        if (pointer == IntPtr.Zero)
        {
            GlobalFree(memory);
            return;
        }

        try
        {
            Marshal.Copy(bytesToCopy, 0, pointer, bytesToCopy.Length);
        }
        finally
        {
            GlobalUnlock(memory);
        }

        if (!OpenClipboard(IntPtr.Zero))
        {
            GlobalFree(memory);
            return;
        }

        try
        {
            if (!EmptyClipboard())
            {
                GlobalFree(memory);
                return;
            }

            if (SetClipboardData(CfUnicodeText, memory) == IntPtr.Zero)
            {
                GlobalFree(memory);
                return;
            }

            // Ownership of the HGLOBAL is transferred to the system after SetClipboardData succeeds.
            _lastStoredText = text;
        }
        finally
        {
            CloseClipboard();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint format, IntPtr memory);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr memory);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr memory);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr memory);
}
