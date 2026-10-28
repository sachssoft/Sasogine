using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Receives messages for an instance installed using SetWindowSubclass.</summary>
/// <param name="window">The native HWND.</param>
/// <param name="message">The window message identifier.</param>
/// <param name="wParam">The message's first argument.</param>
/// <param name="lParam">The message's second argument.</param>
/// <param name="subclassId">The identifier of the installed subclass.</param>
/// <param name="referenceData">The associated native reference data.</param>
/// <returns>The native message result.</returns>
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
internal delegate IntPtr WindowsWindowProcedure(IntPtr window, uint message, UIntPtr wParam,
    IntPtr lParam, UIntPtr subclassId, UIntPtr referenceData);
