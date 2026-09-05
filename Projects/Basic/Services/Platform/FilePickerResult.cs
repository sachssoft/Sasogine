using System;

namespace Sachssoft.Sasogine.Services.Platform;

/// <summary>
/// Represents the result of a file picker operation.
/// </summary>
public class FilePickerResult
{
    /// <summary>
    /// Gets a value indicating whether the user selected one or more files.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the paths selected by the user.
    /// May contain multiple entries when multiple selection is enabled.
    /// </summary>
    public string[] Paths { get; init; } = Array.Empty<string>();
}