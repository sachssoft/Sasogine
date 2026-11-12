using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Virtualizes uniform fixed-size tiles across a configured number of columns.</summary>
public sealed class VirtualTilePanel : VirtualizingPanel
{
    /// <summary>Gets the number of columns in each row.</summary>
    public int Columns { get; init; } = 3;

    /// <summary>Gets the fixed logical tile size.</summary>
    public Size2 TileSize { get; init; } = new(128);

    /// <inheritdoc/>
    protected override Size2 GetSlot(float width, out int columns)
    {
        columns = Columns;
        return TileSize;
    }
}
