using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls;
/// <summary>
/// Renders a scene into a reusable target inside a UI layout slot.
/// </summary>
/// <remarks>Textures assigned to <see cref="Source"/> remain caller-owned.</remarks>
public class SceneViewport : Control, IColorPickSource
{
    private Action<SceneViewportDrawContext>? _render;
    private Texture2D? _source;
    private bool _useDirectRendering;
    private Color? _compositeBackground;
    private float _renderScale = 1;
    private int _multiSampleCount;
    private Color _clearColor = Color.Transparent;
    private RenderTarget2D? _target;
    private RenderTargetColorSource? _colorSource;
    private int _targetSamples;
    private bool _targetReady;
    private static readonly PropertyChangedEventArgs RenderArgs = new(nameof(Render));
    private static readonly PropertyChangedEventArgs SourceArgs = new(nameof(Source));
    private static readonly PropertyChangedEventArgs CompositeArgs = new(nameof(CompositeBackground));
    private static readonly PropertyChangedEventArgs DirectArgs = new(nameof(UseDirectRendering));
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Image";
    /// <summary>
    /// Gets or sets the callback that draws the scene.
    /// </summary>
    public  Action<SceneViewportDrawContext>? Render
    {
        get => _render;
        set
        {
            if (SetNotify(ref _render, value, RenderArgs)) _targetReady = false;
        }
    }
    /// <summary>
    /// Gets or sets the borrowed fallback texture, stretched to the content area.
    /// </summary>
    public Texture2D? Source { get => _source; set => SetNotify(ref _source, value, SourceArgs); }
    /// <summary>
    /// Gets or sets whether drawing invokes Render instead of sampling Source.
    /// </summary>
    public  bool UseDirectRendering
    {
        get => _useDirectRendering;
        set => SetNotify(ref _useDirectRendering, value, DirectArgs);
    }
    /// <summary>
    /// Gets or sets the uniform opaque UI backdrop used to reproduce premultiplied image compositing.
    /// </summary>
    /// <remarks>Null keeps the scene's direct color output. A supplied color must match the underlying UI.</remarks>
    public  Color? CompositeBackground
    {
        get => _compositeBackground;
        set => SetNotify(ref _compositeBackground, value, CompositeArgs);
    }

    /// <summary>Gets or sets the positive render resolution multiplier. The default is one.</summary>
    public float RenderScale
    {
        get => _renderScale;
        set
        {
            if (!float.IsFinite(value) || value <= 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Render scale must be greater than zero and at most one.");
            SetNotify(ref _renderScale, value, RenderScaleArgs);
        }
    }

    /// <summary>Gets or sets the requested offscreen multisample count. Zero disables multisampling.</summary>
    public int MultiSampleCount
    {
        get => _multiSampleCount;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            if (SetNotify(ref _multiSampleCount, value, SamplesArgs)) _targetReady = false;
        }
    }

    /// <summary>Gets or sets the color used to clear the owned render target.</summary>
    public Color ClearColor
    {
        get => _clearColor;
        set => SetNotify(ref _clearColor, value, ClearColorArgs);
    }

    private static readonly PropertyChangedEventArgs RenderScaleArgs = new(nameof(RenderScale));
    private static readonly PropertyChangedEventArgs SamplesArgs = new(nameof(MultiSampleCount));
    private static readonly PropertyChangedEventArgs ClearColorArgs = new(nameof(ClearColor));

    /// <summary>Gets the displayed content bounds in physical window pixels.</summary>
    public Rectangle ScreenBounds
    {
        get
        {
            Bounds2 bounds = ControlTransforms.TransformBounds(ContentBounds, ControlTransforms.GetVisualTransform(this));
            float scale = Workspace?.FrameScale ?? 1;
            Point origin = Workspace?.FrameOrigin ?? Point.Zero;
            int left = (int)Math.Floor(origin.X + bounds.Left * scale);
            int top = (int)Math.Floor(origin.Y + bounds.Top * scale);
            int right = (int)Math.Ceiling(origin.X + bounds.Right * scale);
            int bottom = (int)Math.Ceiling(origin.Y + bounds.Bottom * scale);
            return new Rectangle(left, top, bounds.Width > 0 ? right - left : 0, bounds.Height > 0 ? bottom - top : 0);
        }
    }

    /// <summary>Gets the planned scene resolution in pixels using the last processed workspace layout.</summary>
    public PixelSize2 RenderSize => Source is { } source && !UseDirectRendering
        ? new PixelSize2(source.Width, source.Height)
        : GetRenderSize(Workspace?.FrameScale ?? 1, UseDirectRendering ? 1 : RenderScale);

    /// <summary>Maps a physical window position to a pixel in the displayed scene.</summary>
    /// <param name="screenPosition">The physical window position.</param>
    /// <param name="pixel">The corresponding scene pixel.</param>
    /// <returns>Whether the point lies in the visible scene.</returns>
    public bool TryGetRenderPosition(Point screenPosition, out Point pixel)
    {
        Point origin = Workspace?.FrameOrigin ?? Point.Zero;
        float scale = Workspace?.FrameScale ?? 1;
        return TryMap(new Vector2((screenPosition.X - origin.X) / scale, (screenPosition.Y - origin.Y) / scale),
            RenderSize, out pixel);
    }

    /// <inheritdoc/>
    public bool TryPick(Vector2 position, out Color color)
    {
        color = default;
        if (!HasCurrentTarget || !TryMap(position, RenderSize, out Point pixel) || _target is not { } target)
            return false;
        _colorSource ??= new RenderTargetColorSource(target, new Bounds2(0, 0, target.Width, target.Height));
        return _colorSource.TryPick(new Vector2(pixel.X + .5f, pixel.Y + .5f), out color);
    }

    internal RenderTargetColorSource? ColorSource => _colorSource;

    internal bool IsWaitingForColor(Vector2 position) =>
        !UseDirectRendering && Source is null && Render is not null && !HasCurrentTarget && TryMap(position, RenderSize, out _);

    private bool HasCurrentTarget => _targetReady && !UseDirectRendering && Source is null && Render is not null &&
        _target is { IsDisposed: false } target && _targetSamples == MultiSampleCount &&
        RenderSize.Equals(new PixelSize2(target.Width, target.Height));

    private bool TryMap(Vector2 position, PixelSize2 size, out Point pixel)
    {
        pixel = default;
        if (size.Width <= 0 || size.Height <= 0 || Workspace is not { } workspace ||
            !new Bounds2(0, 0, workspace.Size.Width, workspace.Size.Height).Contains(position.X, position.Y))
            return false;

        Vector2 local = ControlTransforms.ToLayout(this, position);
        Bounds2 content = ContentBounds;
        if (!content.Contains(local.X, local.Y))
            return false;
        for (Control? node = this, child = null; node is not null; child = node, node = node.Parent)
        {
            if (!node.IsVisible || node.Opacity <= 0) return false;
            Vector2 point = ControlTransforms.ToLayout(node, position);
            if (!node.Bounds.Contains(point.X, point.Y) ||
                child is not null && !node.GetChildClip(child).Contains(point.X, point.Y))
                return false;
        }
        pixel = new Point(Math.Min(size.Width - 1, (int)((local.X - content.X) / content.Width * size.Width)),
            Math.Min(size.Height - 1, (int)((local.Y - content.Y) / content.Height * size.Height)));
        return true;
    }

    private PixelSize2 GetRenderSize(float dpi, float scale)
    {
        Bounds2 bounds = ContentBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return PixelSize2.Zero;
        return new PixelSize2(Math.Max(1, checked((int)(Math.Ceiling(bounds.Width * dpi) * scale))),
            Math.Max(1, checked((int)(Math.Ceiling(bounds.Height * dpi) * scale))));
    }
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
        => Source is { } source ? new Size2(source.Width, source.Height) : Size2.Zero;
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        Bounds2 content = new(Padding.Left, Padding.Top, ContentBounds.Width, ContentBounds.Height);
        using (context.PushClipScope(content))
        {
            if (UseDirectRendering && Render is { } render) context.DrawScene(content, render, CompositeBackground);
            else if (Source is { } source) context.DrawTexture(source, source.Bounds, content, Color.White);
            else if (Render is { } draw)
            {
                PixelSize2 size = GetRenderSize(context.Scale, RenderScale);
                if (size.Width <= 0 || size.Height <= 0) return;
                var device = context.GraphicsDevice;
                if (_target is not { IsDisposed: false } target || !ReferenceEquals(target.GraphicsDevice, device) ||
                    target.Width != size.Width || target.Height != size.Height || _targetSamples != MultiSampleCount)
                {
                    ReleaseTarget();
                    _target = new RenderTarget2D(device, size.Width, size.Height, false, SurfaceFormat.Color,
                        DepthFormat.Depth24, MultiSampleCount, RenderTargetUsage.PreserveContents);
                    _targetSamples = MultiSampleCount;
                }
                _targetReady = false;
                context.DrawSceneToTarget(_target!, draw, ClearColor);
                _targetReady = true;
                context.DrawTexture(_target!, _target!.Bounds, content, Color.White);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        ReleaseTarget();
        base.OnDetached();
    }

    private void ReleaseTarget()
    {
        _target?.Dispose();
        _target = null;
        _colorSource = null;
        _targetReady = false;
    }
}
