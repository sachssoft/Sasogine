using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Editor.Tiling.Tools;
using sachssoft.Sasogine.Tiling;

public sealed class EditorToolEllipseRegion : IEditorToolShapeRegion
{
    public bool ContainsFill(Coordinate coordinate, Scope region)
    {
        if (!region.Contains(coordinate))
            return false;

        var (center, radius) = GetEllipseParameters(region);
        float dist = GetNormalizedDistance(coordinate, center, radius);

        // Padding für "vollständige" Pixelellipse
        return dist <= 1.0f;
    }

    public bool ContainsOutline(Coordinate coordinate, Scope region, int thickness)
    {
        if (!region.Contains(coordinate) || thickness <= 0)
            return false;

        var (center, radius) = GetEllipseParameters(region);
        float dist = GetNormalizedDistance(coordinate, center, radius);

        float delta = GetThicknessThreshold(thickness, radius);

        // Nur Werte knapp um die 1.0 herum zählen zum Rand
        return dist >= 1.0f - delta && dist <= 1.0f + delta;
    }

    private static (Vector2 center, Vector2 radius) GetEllipseParameters(Scope region)
    {
        float width = region.Upper.X - region.Lower.X + 1;
        float height = region.Upper.Y - region.Lower.Y + 1;

        Vector2 center = new(region.Lower.X + width / 2f - 0.5f, region.Lower.Y + height / 2f - 0.5f);
        Vector2 radius = new(width / 2f, height / 2f);

        return (center, radius);
    }

    private static float GetNormalizedDistance(Coordinate point, Vector2 center, Vector2 radius)
    {
        float dx = (point.X - center.X) / (radius.X != 0 ? radius.X : 1);
        float dy = (point.Y - center.Y) / (radius.Y != 0 ? radius.Y : 1);
        return dx * dx + dy * dy;
    }

    private static float GetThicknessThreshold(int thickness, Vector2 radius)
    {
        float avgRadius = (radius.X + radius.Y) / 2f;
        if (avgRadius <= 0f) return 0.01f;

        // „Pixel-Einheiten“ in Ellipsen-Normalform umrechnen
        float normed = thickness / avgRadius;

        // Quadratischer Abstand, da wir mit dist² arbeiten
        return normed * 0.5f;
    }
}
