using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Provides information about capabilities supported by the current runtime
/// and platform.
/// </summary>
public static class RuntimeCapabilities
{
    /// <summary>
    /// Gets a value indicating whether reflection-based functionality
    /// is supported by the current runtime and platform.
    /// </summary>
    public static bool IsReflectionSupported =>
        RuntimeFeature.IsDynamicCodeSupported &&
        IsPlatformReflectionSupported;

    /// <summary>
    /// Gets a value indicating whether reflection-based functionality
    /// is supported on the current operating system platform.
    /// </summary>
    public static bool IsPlatformReflectionSupported =>
        OperatingSystem.IsWindows() ||
        OperatingSystem.IsLinux() ||
        OperatingSystem.IsMacOS() ||
        OperatingSystem.IsAndroid();

    /// <summary>
    /// Ensures that reflection-based functionality is supported by the
    /// current runtime and platform.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current
    /// runtime or platform.
    /// </exception>
    public static void EnsureReflectionSupported()
    {
        if (!IsReflectionSupported)
            throw new PlatformNotSupportedException(
                "Reflection-based functionality is not supported by the current runtime or platform.");
    }
}