using sachssoft.Sasogine.Geometry;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public interface IEditorToolShapeWithTransform
{
    RotationFacing Rotation { get; set; }

    FlipMode Flip { get; set; }
}
