using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

public readonly struct SelectionToolLayerContext
{
    public SelectionToolLayerContext(
        bool enableSnap,
        Size2 gridSize,
        float handleSize)
    {
        EnableSnap = enableSnap;
        GridSize = gridSize;
        HandleSize = handleSize;
    }

    public bool EnableSnap { get; }
    public Size2 GridSize { get; }
    public float HandleSize { get; }
}