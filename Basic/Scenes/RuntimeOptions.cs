using System;

namespace Sachssoft.Sasogine.Scenes;

/// <summary>
/// Defines optional runtime features that can be enabled for a runtime environment.
/// Multiple options can be combined using bitwise operations.
/// </summary>
[Flags]
public enum RuntimeOptions
{
    /// <summary>
    /// No additional runtime options are enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Enables debug features and diagnostic functionality useful during development.
    /// </summary>
    Debug = 1 << 0

    // Weitere Einträge sind aus Kompatibilitätsgründen möglich.
    // Profiler usw. werden bei Bedarf später ergänzt.
}