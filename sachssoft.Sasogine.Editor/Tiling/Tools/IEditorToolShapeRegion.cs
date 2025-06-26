
using sachssoft.Sasogine.Tiling;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public interface IEditorToolShapeRegion : IEditorToolShapeGeometry
{
    /// <summary>
    /// Prüft, ob ein Punkt innerhalb der gefüllten Fläche der Region liegt.
    /// </summary>
    bool ContainsFill(Coordinate coordinate, Scope region);

    /// <summary>
    /// Prüft, ob ein Punkt innerhalb der äußeren Umrandung liegt (Outline mit gegebener Dicke).
    /// </summary>
    bool ContainsOutline(Coordinate coordinate, Scope region, int thickness);
}
