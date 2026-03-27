using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Tiling;

namespace Sachssoft.Sasogine.Editor.Tiles.Tools;

public sealed class EditorToolRectangleRegion : IEditorToolShapeRegion
{
    public bool ContainsFill(Coordinate coordinate, Scope region)
    {
        return region.Contains(coordinate);
    }

    public bool ContainsOutline(Coordinate coordinate, Scope region, int thickness)
    {
        if (thickness <= 0 || !region.Contains(coordinate))
            return false;

        int left = region.Lower.X;
        int top = region.Lower.Y;
        int right = region.Upper.X - 1;   // Inclusive Bereich
        int bottom = region.Upper.Y - 1;  // Inclusive Bereich

        int x = coordinate.X;
        int y = coordinate.Y;

        // Abstand des Punkts zum jeweiligen Rand
        int dxLeft = x - left;
        int dxRight = right - x;
        int dyTop = y - top;
        int dyBottom = bottom - y;

        // Punkt liegt innerhalb der Randzone
        return dxLeft < thickness || dxRight < thickness || dyTop < thickness || dyBottom < thickness;
    }

}
