namespace Sachssoft.Engine.Controls;
/// <summary>Describes a borrowed texture region. Its owner retains responsibility for disposal.</summary>
public sealed class AssetPreview
{
    /// <summary>Creates a preview for a texture or atlas region.</summary>
    public AssetPreview(Texture2D texture, Rectangle? source = null)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
        Source = source ?? texture.Bounds;
        if (Source.Width <= 0 || Source.Height <= 0 || !texture.Bounds.Contains(Source))
            throw new ArgumentOutOfRangeException(nameof(source));
    }
    /// <summary>Gets the caller-owned texture.</summary>
    public Texture2D Texture { get; }
    /// <summary>Gets the texture region.</summary>
    public Rectangle Source { get; }
}
