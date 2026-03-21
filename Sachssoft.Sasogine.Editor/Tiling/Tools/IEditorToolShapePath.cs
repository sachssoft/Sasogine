using Sachssoft.Sasogine.Tiling;

namespace Sachssoft.Sasogine.Editor.Tiling.Tools;

public interface IEditorToolShapePath : IEditorToolShapeGeometry
{
    bool Contains(Coordinate coordinate, Coordinate start, Coordinate end, int thickness);
}
