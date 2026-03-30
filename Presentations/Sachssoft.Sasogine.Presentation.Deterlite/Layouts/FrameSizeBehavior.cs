namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public enum FrameSizeBehavior
    {
        Auto,       // Höhe/Breite wird anhand von Content / MeasureOverride berechnet
        Stretch,    // Streckt sich, um verfügbaren Platz auszufüllen (wie VerticalStretch = true)
        Zero        // Ignoriert Inhalt, bleibt 0
    }
}
