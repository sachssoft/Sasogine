using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Editor.Tiles.Tools;

public interface IEditorToolShapeWithTransform
{
    RotationFacing Rotation { get; set; }

    FlipMode Flip { get; set; }
}
