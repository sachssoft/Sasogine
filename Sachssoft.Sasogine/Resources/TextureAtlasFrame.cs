namespace Sachssoft.Sasogine.Resources;

using Microsoft.Xna.Framework;

public class TextureAtlasFrame
{
    /// <summary>
    /// Logischer Name des Frames (z. B. "hero_idle_01.png")
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Quellrechteck im TextureAtlas (Pixel-Koordinaten)
    /// </summary>
    public Rectangle SourceRectangle { get; }

    /// <summary>
    /// Optional: Ursprungs-/Pivotpunkt relativ zum Frame.
    /// Wird beim Zeichnen als Rotations-/Scale-Referenz genutzt.
    /// </summary>
    public Vector2 Origin { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gibt an, ob der Frame im Atlas um 90° gedreht gespeichert ist.
    /// (Starling/LibGDX unterstützen "rotated" Sprites, spart Platz)
    /// </summary>
    public bool Rotated { get; }

    /// <summary>
    /// Optional: Originalgröße des Sprites vor dem Trimmen (Starling frameWidth/frameHeight)
    /// </summary>
    public Rectangle OriginalFrame { get; }

    /// <summary>
    /// Größe des zugrunde liegenden Textures (wird von TextureAtlas gesetzt)
    /// </summary>
    internal int TextureWidth { get; set; }
    internal int TextureHeight { get; set; }

    // Normierte UV-Koordinaten → praktisch für eigene Shader oder manuelles Batching
    public Vector2 UVTopLeft => new Vector2(
        (float)SourceRectangle.X / TextureWidth,
        (float)SourceRectangle.Y / TextureHeight);

    public Vector2 UVBottomRight => new Vector2(
        (float)(SourceRectangle.X + SourceRectangle.Width) / TextureWidth,
        (float)(SourceRectangle.Y + SourceRectangle.Height) / TextureHeight);

    // Breite & Höhe als Kürzel
    public int Width => SourceRectangle.Width;
    public int Height => SourceRectangle.Height;

    // Größe als Vector2
    public Vector2 Size => new Vector2(SourceRectangle.Width, SourceRectangle.Height);

    public TextureAtlasFrame(
        string name,
        Rectangle sourceRect,
        bool rotated = false,
        Rectangle originalFrame = default)
    {
        Name = name;
        SourceRectangle = sourceRect;
        Rotated = rotated;
        OriginalFrame = originalFrame;
    }
}
