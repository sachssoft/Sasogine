using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>Matches the three-byte SDL2 version structure used by the window information API.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct WindowsSdlVersion
{
    /// <summary>Stores the major library version.</summary>
    internal byte Major;

    /// <summary>Stores the minor library version.</summary>
    internal byte Minor;

    /// <summary>Stores the patch library version.</summary>
    internal byte Patch;
}
