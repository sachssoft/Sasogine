namespace Sachssoft.Sasogine.Services.Platform;

/// <summary>
/// Provides configuration for a file picker operation.
/// </summary>
public class FilePickerContext
{
    /// <summary>
    /// Gets or sets the title displayed by the file picker.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the directory initially displayed by the file picker.
    /// </summary>
    public string? InitialDirectory { get; init; }

    /// <summary>
    /// Gets or sets the default file name.
    /// </summary>
    public string? DefaultFileName { get; init; }

    /// <summary>
    /// Gets or sets the file filters.
    /// For example, <c>*.txt</c> or <c>*.png</c>.
    /// </summary>
    public string[]? Filters { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple files may be selected.
    /// </summary>
    public bool AllowMultiple { get; init; }
}