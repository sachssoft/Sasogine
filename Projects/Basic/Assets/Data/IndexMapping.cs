namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Represents a mapping between a numeric frame index and its associated name.
/// </summary>
public sealed class IndexMapping
{
    /// <summary>
    /// Gets or sets the numeric frame index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the associated frame name.
    /// </summary>
    public string? Name { get; set; }
}