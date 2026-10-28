using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Matches SDL2 system window information, including its 64-byte native information union.</summary>
[StructLayout(LayoutKind.Explicit, Size = 72)]
internal struct WindowsSdlWindowInfo
{
    /// <summary>Stores the Windows HWND at the start of the native information union.</summary>
    [FieldOffset(8)]
    internal IntPtr Window;

    /// <summary>Stores the version required by SDL_GetWindowWMInfo.</summary>
    [FieldOffset(0)]
    internal WindowsSdlVersion Version;

    /// <summary>Stores the native window subsystem; Windows has identifier one.</summary>
    [FieldOffset(4)]
    internal int Subsystem;
}
