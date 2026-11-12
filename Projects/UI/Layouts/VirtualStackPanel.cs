using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Virtualizes a single column of fixed-height items.</summary>
public sealed class VirtualStackPanel : VirtualizingPanel
{
    /// <summary>Gets the fixed height of each item in logical units.</summary>
    public float ItemHeight { get; init; } = 32;

    /// <inheritdoc/>
    protected override Size2 GetSlot(float width, out int columns)
    {
        columns = 1;
        return new Size2(width, ItemHeight);
    }
}
