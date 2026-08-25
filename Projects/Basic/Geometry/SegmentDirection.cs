namespace Sachssoft.Sasogine.Geometry
{
    public enum SegmentDirection
    {
        None,       // Einfaches Quad
        Horizontal, // Unterteilung nur X-Richtung
        Vertical,   // Unterteilung nur Y-Richtung
        Both        // Vollständiges Gitter
    }
}
