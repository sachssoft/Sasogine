namespace Sachssoft.Engine.Controls;
/// <summary>Samples a caller-owned Color render target and caches a small magnifier neighborhood.</summary>
/// <remarks>The target must be unbound before sampling. Viewport uses workspace logical coordinates.
/// Filtering, tone mapping and display color transforms belong to the caller's rendered target.</remarks>
public sealed class RenderTargetColorSource : IColorPickSource
{
    private readonly Color[] _readback = new Color[81];
    private readonly Color[] _magnifier = new Color[81];
    /// <summary>Creates a source for the target's entire image or an optional texture region.</summary>
    public RenderTargetColorSource(RenderTarget2D target, Bounds2 viewport, Rectangle? source = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        Target = target;
        Viewport = viewport;
        Source = source;
    }
    /// <summary>Gets the borrowed target. The application owns its lifetime and rendering.</summary>
    public RenderTarget2D Target { get; }
    /// <summary>Gets or sets the displayed image area in workspace logical coordinates.</summary>
    public Bounds2 Viewport { get; set; }
    /// <summary>Gets or sets the sampled texture region; null uses the whole target.</summary>
    public Rectangle? Source { get; set; }
    /// <summary>Gets or sets whether stored premultiplied RGB should be converted to straight RGB.</summary>
    public bool UnpremultiplyAlpha { get; set; } = true;
    /// <summary>Gets the last successfully sampled target pixel.</summary>
    public Point LastPixel { get; private set; }
    /// <summary>Gets the last 9 by 9 straight-color neighborhood; edge pixels are clamped.</summary>
    public ReadOnlySpan<Color> MagnifierPixels => _magnifier;
    /// <inheritdoc/>
    public bool TryPick(Vector2 position, out Color color)
    {
        color = default;
        Bounds2 b = Viewport;
        Rectangle region = Source ?? Target.Bounds;
        if (Target.IsDisposed || Target.Format != SurfaceFormat.Color || !float.IsFinite(b.X) || !float.IsFinite(b.Y) || !float.IsFinite(b.Width) || !float.IsFinite(b.Height)
            || b.Width <= 0 || b.Height <= 0 || !float.IsFinite(position.X) || !float.IsFinite(position.Y)
            || position.X < b.X || position.Y < b.Y || position.X >= b.Right || position.Y >= b.Bottom
            || region.Width <= 0 || region.Height <= 0 || !Target.Bounds.Contains(region)) return false;
        foreach (RenderTargetBinding binding in Target.GraphicsDevice.GetRenderTargets())
            if (ReferenceEquals(binding.RenderTarget, Target)) return false;
        int x = region.X + Math.Min(region.Width - 1, (int)((position.X - b.X) / b.Width * region.Width));
        int y = region.Y + Math.Min(region.Height - 1, (int)((position.Y - b.Y) / b.Height * region.Height));
        int left = Math.Max(region.Left, x - 4), top = Math.Max(region.Top, y - 4);
        int right = Math.Min(region.Right, x + 5), bottom = Math.Min(region.Bottom, y + 5);
        int width = right - left, height = bottom - top;
        Target.GetData(0, new Rectangle(left, top, width, height), _readback, 0, width * height);
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
            {
                int px = Math.Clamp(x + column - 4, left, right - 1) - left;
                int py = Math.Clamp(y + row - 4, top, bottom - 1) - top;
                Color sample = _readback[py * width + px];
                _magnifier[row * 9 + column] = UnpremultiplyAlpha ? ToStraight(sample) : sample;
            }
        LastPixel = new Point(x, y);
        color = _magnifier[40];
        return true;
    }
    private static Color ToStraight(Color value)
    {
        if (value.A == 0) return Color.Transparent;
        if (value.A == 255) return value;
        return new Color((byte)Math.Min(255, (value.R * 255 + value.A / 2) / value.A),
            (byte)Math.Min(255, (value.G * 255 + value.A / 2) / value.A),
            (byte)Math.Min(255, (value.B * 255 + value.A / 2) / value.A), value.A);
    }
}
