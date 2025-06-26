using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Tiling;
using System;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public sealed class EditorToolLinePath : IEditorToolShapePath
{
    public bool Contains(Coordinate coordinate, Coordinate start, Coordinate end, int thickness)
    {
        if (thickness <= 0)
            return false;

        return IsPointNearLine(
            coordinate.X, coordinate.Y,
            start.X, start.Y,
            end.X, end.Y,
            thickness);
    }

    private static bool IsPointNearLine(int px, int py, int x0, int y0, int x1, int y1, int thickness)
    {
        float fx = px;
        float fy = py;

        float dx = x1 - x0;
        float dy = y1 - y0;

        if (dx == 0 && dy == 0)
        {
            // Linie ist ein Punkt
            float dist = Distance(fx, fy, x0, y0);
            return dist <= thickness * 0.5f;
        }

        // Lotfußpunkt t
        float t = ((fx - x0) * dx + (fy - y0) * dy) / (dx * dx + dy * dy);
        t = float.Clamp(t, 0f, 1f);

        float lx = x0 + t * dx;
        float ly = y0 + t * dy;

        float distToLine = Distance(fx, fy, lx, ly);
        return distToLine <= thickness * 0.5f;
    }

    private static float Distance(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return float.Sqrt(dx * dx + dy * dy);
    }
}
