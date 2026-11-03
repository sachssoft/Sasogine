using Sachssoft.Engine.Graphics;
using Sachssoft.Engine.Graphics.Brushes;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Sasogine.ShapeBatchTests;

/// <summary>Simulates a custom brush failing after geometry has already started.</summary>
internal sealed class ThrowingBrush : Brush
{
    protected internal override void Apply(ref ShapeVertex vertex, in BrushVertexContext context) =>
        throw new InvalidOperationException("Intentional brush failure.");
}
