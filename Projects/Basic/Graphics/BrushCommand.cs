
namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Describes a contiguous index range rendered with the same brush state.
/// </summary>
internal readonly struct BrushCommand
{
    public BrushCommand(Brush brush, int startIndex, int indexCount)
    {
        Brush = brush;
        StartIndex = startIndex;
        IndexCount = indexCount;
    }

    public Brush Brush { get; }
    public int StartIndex { get; }
    public int IndexCount { get; }
}
