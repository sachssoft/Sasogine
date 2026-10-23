using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Draws a UI tree through a native frame batch with local coordinates and nested clipping.
/// </summary>
/// <remarks>
/// The context owns both rendering paths and their graphics resources. The caller owns fonts.
/// Native fonts and built-in geometry use the frame batch. Legacy fonts and custom SpriteBatch
/// access create explicit ordering barriers. Custom controls must not begin/end batches or change device state.
/// Colors use premultiplied alpha. Coordinates support translation and uniform DPI scaling.
/// </remarks>
public sealed class ControlDrawContext : GameContext, IDisposable
{
    private readonly SamplerState[] _sceneSamplers = new SamplerState[8];
    private readonly Texture?[] _sceneTextures = new Texture?[8];
    private RenderTargetBinding[] _sceneTargets = Array.Empty<RenderTargetBinding>();
    private RenderTargetBinding[] _sceneCurrentTargets = Array.Empty<RenderTargetBinding>();
    private readonly RasterizerState _sceneRasterizer = new() { CullMode = CullMode.None, ScissorTestEnable = true };
    private bool _drawingScene;
    private SpriteBatch? _sceneCompositeBatch;
    private readonly BlendState _sceneCompositeBlend = new()
    {
        ColorSourceBlend = Blend.InverseDestinationAlpha, ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One, AlphaDestinationBlend = Blend.Zero
    };
    private SpriteBatch? _batch;
    private readonly ControlFrameBatch? _frameBatch;
    private readonly Texture2D _white;
    private readonly RasterizerState _rasterizer;
    private bool _active, _batchActive, _disposed, _legacyControl;
    private Rectangle _viewport;
    private float _scale;
    private float _opacity = 1;
    private Bounds2 _currentBounds, _currentClip;
    private UIRegistry? _registry;
    private Viewport _previousViewport;
    private Rectangle _previousScissor;
    private BlendState? _previousBlend;
    private DepthStencilState? _previousDepth;
    private RasterizerState? _previousRasterizer;
    private SamplerState? _previousSampler;
    /// <summary>
    /// Creates persistent UI drawing resources for the application graphics device.
    /// </summary>
    public ControlDrawContext(GameApplicationBase application, IControlFont defaultFont, Rectangle viewport, float scale = 1) : this(application, defaultFont, viewport, scale, ControlRenderer.FrameBatch)
    {
    }

    /// <summary>
    /// Creates a context with an explicit renderer for compatibility or controlled comparison.
    /// </summary>
    public ControlDrawContext(GameApplicationBase application, IControlFont defaultFont, Rectangle viewport, float scale, ControlRenderer renderer) : base(application)
    {
        if (renderer is not (ControlRenderer.FrameBatch or ControlRenderer.SpriteBatch))
            throw new ArgumentOutOfRangeException(nameof(renderer));
        Renderer = renderer;
        DefaultFont = defaultFont ?? throw new ArgumentNullException(nameof(defaultFont));
        SetViewport(viewport, scale);
        if (renderer == ControlRenderer.FrameBatch)
            _frameBatch = new ControlFrameBatch(GraphicsDevice);
        else
            _batch = new SpriteBatch(GraphicsDevice);
        _white = new Texture2D(GraphicsDevice, 1, 1);
        _white.SetData(new[] { Color.White });
        if (_frameBatch is not null) _frameBatch.OpaqueWhiteTexture = _white;
        _rasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };
    }

    /// <summary>
    /// Gets the selected renderer.
    /// </summary>
    public ControlRenderer Renderer { get; }
    /// <summary>
    /// Gets native draw calls from the current or most recently completed frame.
    /// </summary>
    public int FrameDrawCalls => _frameBatch?.DrawCalls ?? 0;

    /// <summary>
    /// Gets the active native batch. Access ends an active compatibility SpriteBatch.
    /// </summary>
    public ControlFrameBatch FrameBatch
    {
        get
        {
            EnsureDrawing();
            if (_frameBatch is null)
                throw new InvalidOperationException("This context uses the legacy renderer.");
            EndLegacy();
            _legacyControl = false;
            return _frameBatch;
        }
    }

    /// <summary>
    /// Gets the default caller-owned font, matching the layout context.
    /// </summary>
    public IControlFont DefaultFont { get; }
    /// <summary>
    /// Gets the logical viewport size.
    /// </summary>
    public Size2 LogicalSize => new(_viewport.Width / _scale, _viewport.Height / _scale);
    /// <summary>
    /// Gets the physical viewport used for drawing.
    /// </summary>
    public Rectangle Viewport => _viewport;
    /// <summary>
    /// Gets physical pixels per logical UI unit.
    /// </summary>
    public float Scale => _scale;

    /// <summary>
    /// Gets the compatibility batch, flushing native work first. Prefer FrameBatch for new custom drawing.
    /// </summary>
    public SpriteBatch SpriteBatch
    {
        get
        {
            EnsureDrawing();
            _legacyControl = true;
            EnsureLegacy();
            return _batch!;
        }
    }

    /// <summary>
    /// Gets the composed opacity for custom drawing.
    /// </summary>
    public float Opacity => _opacity;

    internal void Bind(UIRegistry registry) => _registry = registry;
    /// <summary>
    /// Draws an explicitly resolved skin part; false permits a procedural fallback.
    /// </summary>
    public bool DrawSkinPart(Control control, string part, Bounds2 bounds, string state = "Normal", Color? tint = null)
    {
        if (_registry?.Skin is not ControlSkin skin || skin.Texture is null || !skin.Find(control, part, state, _registry.Styles, out SkinRegion? region))
            return false;
        if (part == "Background" && control.BorderThickness > 0 && (control.IsStyleLocal("BorderColor") || _registry.SkinColors.TryGetValue(control.StandardStyleName, out var borderColors) && borderColors.TryGetValue("BorderColor", out Color borderBaseline) && control.BorderColor != borderBaseline))
            return false;
        if (part is "Background" or "Focus")
        {
            string key = part == "Focus" ? "FocusColor" : state.Contains("Disabled", StringComparison.Ordinal) ? "DisabledBackground" : state.Contains("Pressed", StringComparison.Ordinal) ? "PressedBackground" : state.Contains("Hover", StringComparison.Ordinal) ? "HoverBackground" : "Background";
            Color current = key switch
            {
                "FocusColor" => control.FocusColor,
                "DisabledBackground" => control.DisabledBackground,
                "PressedBackground" => control.PressedBackground,
                "HoverBackground" => control.HoverBackground,
                _ => control.Background
            };
            if (control.IsStyleLocal(key) || _registry.SkinColors.TryGetValue(control.StandardStyleName, out var colors) && colors.TryGetValue(key, out Color baseline) && current != baseline)
                return false;
        }

        DrawRegion(skin, region!, bounds, tint ?? Color.White);
        return true;
    }

    /// <summary>
    /// Draws a named atlas region when an active skin is attached.
    /// </summary>
    public bool DrawSkinRegion(string name, Bounds2 bounds, Color? tint = null)
    {
        if (_registry?.Skin is not ControlSkin skin || skin.Texture is null)
            return false;
        DrawRegion(skin, skin[name], bounds, tint ?? Color.White);
        return true;
    }

    /// <summary>
    /// Draws an explicit orientation or shared style part.
    /// </summary>
    public bool DrawSkinStylePart(string style, string part, Bounds2 bounds, string state = "Normal", Color? tint = null)
    {
        if (_registry?.Skin is not ControlSkin skin || skin.Texture is null || !skin.FindStyle(style, part, state, _registry.Styles, out SkinRegion? region))
            return false;
        DrawRegion(skin, region!, bounds, tint ?? Color.White);
        return true;
    }

    /// <summary>
    /// Temporarily clips local drawing, restoring the previous nested clip on disposal.
    /// </summary>
    public IDisposable PushClip(Bounds2 localBounds) => PushClipScope(localBounds);
    /// <summary>
    /// Creates an allocation-free local clip scope. Dispose in reverse nesting order without copying it.
    /// </summary>
    public ControlClipScope PushClipScope(Bounds2 localBounds)
    {
        EnsureDrawing();
        Bounds2 previous = _currentClip;
        var absolute = new Bounds2(_currentBounds.X + localBounds.X, _currentBounds.Y + localBounds.Y, localBounds.Width, localBounds.Height);
        SetControl(_currentBounds, Control.Intersect(previous, absolute), _opacity, true);
        return new ControlClipScope(this, previous);
    }

    internal void RestoreClip(Bounds2 previous)
    {
        EnsureDrawing();
        SetControl(_currentBounds, previous, _opacity, true);
    }

    private void DrawRegion(ControlSkin skin, SkinRegion region, Bounds2 destination, Color color)
    {
        EnsureDrawing();
        if (destination.Width <= 0 || destination.Height <= 0)
            return;
        Rectangle source = region.Source;
        void Draw(Rectangle s, Bounds2 d)
        {
            if (s.Width <= 0 || s.Height <= 0 || d.Width <= 0 || d.Height <= 0)
                return;
            DrawSprite(skin.Texture!, new Vector2(d.X, d.Y), s, color * _opacity, 0, Vector2.Zero, new Vector2(d.Width / s.Width, d.Height / s.Height), SpriteEffects.None, 0);
        }

        if (region.Mode == "Fixed")
        {
            float scale = MathF.Min(destination.Width / source.Width, destination.Height / source.Height);
            Draw(source, new Bounds2(destination.X + (destination.Width - source.Width * scale) / 2, destination.Y + (destination.Height - source.Height * scale) / 2, source.Width * scale, source.Height * scale));
            return;
        }

        if (region.Mode == "Tile")
        {
            for (float y = 0; y < destination.Height; y += source.Height)
                for (float x = 0; x < destination.Width; x += source.Width)
                {
                    int w = (int)MathF.Min(source.Width, destination.Width - x), h = (int)MathF.Min(source.Height, destination.Height - y);
                    Draw(new Rectangle(source.X, source.Y, w, h), new Bounds2(destination.X + x, destination.Y + y, w, h));
                }

            return;
        }

        if (region.Mode != "NineSlice")
        {
            Draw(source, destination);
            return;
        }

        PixelInsets2 p = region.Slice;
        float sx = p.Horizontal > 0 ? MathF.Min(1, destination.Width / p.Horizontal) : 1;
        float sy = p.Vertical > 0 ? MathF.Min(1, destination.Height / p.Vertical) : 1;
        Span<int> xs = stackalloc int[]
        {
            0,
            p.Left,
            source.Width - p.Right,
            source.Width
        };
        Span<int> ys = stackalloc int[]
        {
            0,
            p.Top,
            source.Height - p.Bottom,
            source.Height
        };
        Span<float> dx = stackalloc float[]
        {
            destination.X,
            destination.X + p.Left * sx,
            destination.Right - p.Right * sx,
            destination.Right
        };
        Span<float> dy = stackalloc float[]
        {
            destination.Y,
            destination.Y + p.Top * sy,
            destination.Bottom - p.Bottom * sy,
            destination.Bottom
        };
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                Draw(new Rectangle(source.X + xs[x], source.Y + ys[y], xs[x + 1] - xs[x], ys[y + 1] - ys[y]), new Bounds2(dx[x], dy[y], dx[x + 1] - dx[x], dy[y + 1] - dy[y]));
    }

    /// <summary>
    /// Sets the target viewport and positive DPI scale between draw calls.
    /// </summary>
    public void SetViewport(Rectangle viewport, float scale = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_active)
            throw new InvalidOperationException("Cannot resize during drawing.");
        if (!float.IsFinite(scale) || scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale));
        if (viewport.Width <= 0 || viewport.Height <= 0 || viewport.X < 0 || viewport.Y < 0 || (long)viewport.X + viewport.Width > int.MaxValue || (long)viewport.Y + viewport.Height > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(viewport));
        if (!float.IsFinite(viewport.Width / scale) || !float.IsFinite(viewport.Height / scale))
            throw new ArgumentOutOfRangeException(nameof(scale));
        _viewport = viewport;
        _scale = scale;
    }

    /// <summary>
    /// Draws a filled rectangle in the current control's local coordinates.
    /// </summary>
    public void FillRectangle(Bounds2 bounds, Color color)
    {
        EnsureDrawing();
        if (bounds.Width <= 0 || bounds.Height <= 0 || color.A == 0)
            return;
        DrawSprite(_white, new Vector2(bounds.X, bounds.Y), null, color * _opacity, 0, Vector2.Zero, new Vector2(bounds.Width, bounds.Height), SpriteEffects.None, 0);
    }

    /// <summary>
    /// Draws a line in control-local coordinates through the active ordered batch.
    /// </summary>
    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        EnsureDrawing();
        Vector2 delta = end - start;
        float length = delta.Length();
        if (!float.IsFinite(length) || !float.IsFinite(thickness) || thickness <= 0)
            throw new ArgumentOutOfRangeException(nameof(thickness));
        if (length == 0)
            return;
        DrawSprite(_white, start, null, color * _opacity, MathF.Atan2(delta.Y, delta.X), new Vector2(0, .5f), new Vector2(length, thickness), SpriteEffects.None, 0);
    }

    /// <summary>
    /// Draws a borrowed texture crop in local coordinates using the current clip and opacity.
    /// </summary>
    public void DrawTexture(Texture2D texture, Rectangle source, Bounds2 destination, Color tint)
    {
        EnsureDrawing();
        ArgumentNullException.ThrowIfNull(texture);
        if (source.Width <= 0 || source.Height <= 0 || destination.Width <= 0 || destination.Height <= 0)
            return;
        DrawSprite(texture, new Vector2(destination.X, destination.Y), source, tint * _opacity, 0, Vector2.Zero, new Vector2(destination.Width / source.Width, destination.Height / source.Height), SpriteEffects.None, 0);
    }

    /// <summary>
    /// Draws an inward rectangular border in local coordinates.
    /// </summary>
    public void StrokeRectangle(Bounds2 bounds, Color color, float thickness)
    {
        EnsureDrawing();
        if (thickness <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            return;
        float t = MathF.Min(thickness, MathF.Min(bounds.Width, bounds.Height) / 2);
        FillRectangle(new Bounds2(bounds.X, bounds.Y, bounds.Width, t), color);
        FillRectangle(new Bounds2(bounds.X, bounds.Bottom - t, bounds.Width, t), color);
        FillRectangle(new Bounds2(bounds.X, bounds.Y + t, t, bounds.Height - 2 * t), color);
        FillRectangle(new Bounds2(bounds.Right - t, bounds.Y + t, t, bounds.Height - 2 * t), color);
    }

    /// <summary>
    /// Draws text using the current local origin and composed opacity.
    /// </summary>
    public void DrawText(IControlFont font, string text, Vector2 position, Color color)
    {
        EnsureDrawing();
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);
        if (_frameBatch is not null && !_legacyControl && font is IControlBatchFont batchedFont)
        {
            EndLegacy();
            batchedFont.Draw(_frameBatch, text, position, color * _opacity);
        }
        else
        {
            _legacyControl = true;
            EnsureLegacy();
            font.Draw(_batch!, text, position, color * _opacity);
        }
    }

    internal void Begin()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_active)
            throw new InvalidOperationException("Context is already drawing.");
        _previousViewport = GraphicsDevice.Viewport;
        _previousScissor = GraphicsDevice.ScissorRectangle;
        _previousBlend = GraphicsDevice.BlendState;
        _previousDepth = GraphicsDevice.DepthStencilState;
        _previousRasterizer = GraphicsDevice.RasterizerState;
        _previousSampler = GraphicsDevice.SamplerStates[0];
        _active = true;
        try
        {
            GraphicsDevice.Viewport = new Viewport(_viewport);
            _frameBatch?.Begin(_viewport.Width, _viewport.Height);
        }
        catch
        {
            End();
            throw;
        }
    }

    internal void SetControl(Bounds2 bounds, Bounds2 clip, float opacity, bool preserveCompatibility = false)
    {
        bool compatibility = preserveCompatibility && _legacyControl;
        EndLegacy();
        _legacyControl = compatibility;
        _opacity = opacity;
        _currentBounds = bounds;
        _currentClip = clip;
        if (_frameBatch is not null)
        {
            Rectangle pixels = ToPixelClip(clip, _viewport, _scale);
            _frameBatch.SetControl(new Vector2(bounds.X, bounds.Y), _scale, new Bounds2(pixels.X - _viewport.X, pixels.Y - _viewport.Y, pixels.Width, pixels.Height));
            if (_legacyControl)
                EnsureLegacy();
        }
        else
            EnsureLegacy();
    }

    private void EnsureLegacy()
    {
        if (_batchActive)
            return;
        _frameBatch?.Flush();
        _batch ??= new SpriteBatch(GraphicsDevice);
        GraphicsDevice.ScissorRectangle = ToPixelClip(_currentClip, _viewport, _scale);
        Matrix transform = Matrix.CreateTranslation(_currentBounds.X, _currentBounds.Y, 0) * Matrix.CreateScale(_scale, _scale, 1);
        _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, _rasterizer, null, transform);
        _batchActive = true;
    }

    private void EndLegacy()
    {
        if (!_batchActive)
            return;
        _batchActive = false;
        _batch!.End();
    }

    private void DrawSprite(Texture2D texture, Vector2 position, Rectangle? source, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth)
    {
        if (_frameBatch is not null && !_legacyControl)
        {
            EndLegacy();
            _frameBatch.Draw(texture, position, source, color, rotation, origin, scale, effects, depth);
        }
        else
        {
            EnsureLegacy();
            _batch!.Draw(texture, position, source, color, rotation, origin, scale, effects, depth);
        }
    }

    /// <summary>
    /// Flushes preceding UI work, invokes a direct scene and restores the UI graphics state.
    /// </summary>
    /// <remarks>The callback receives physical bounds and clipping. Scene shaders own their vertex bindings;
    /// this method restores render targets, viewport, blend/depth/rasterizer state and texture/sampler slots 0–7.
    /// Call SetFrameTime before drawing a scene viewport.
    /// Callback clears must preserve pixels outside Clip. This operation is not reentrant.</remarks>
    public void DrawScene(Bounds2 localBounds, Action<SceneViewportDrawContext> render)
        => DrawScene(localBounds, render, null);

    /// <summary>
    /// Draws a direct scene, optionally reproducing image compositing over a uniform opaque backdrop.
    /// </summary>
    /// <remarks>Composite RGB is scene RGB plus backdrop RGB times inverse destination alpha.
    /// This preserves the previous premultiplied Image result without reading an off-screen scene texture.</remarks>
    public void DrawScene(Bounds2 localBounds, Action<SceneViewportDrawContext> render, Color? compositeBackground)
    {
        EnsureDrawing();
        ArgumentNullException.ThrowIfNull(render);
        if (_drawingScene) throw new InvalidOperationException("Direct scene rendering cannot be reentered.");
        Rectangle clip = ToPixelClip(_currentClip, _viewport, _scale);
        int left = (int)Math.Floor(_viewport.X + (_currentBounds.X + localBounds.X) * _scale);
        int top = (int)Math.Floor(_viewport.Y + (_currentBounds.Y + localBounds.Y) * _scale);
        int right = (int)Math.Ceiling(_viewport.X + (_currentBounds.X + localBounds.Right) * _scale);
        int bottom = (int)Math.Ceiling(_viewport.Y + (_currentBounds.Y + localBounds.Bottom) * _scale);
        Rectangle bounds = new(left, top, right - left, bottom - top);
        clip = Rectangle.Intersect(clip, bounds);
        if (bounds.Width <= 0 || bounds.Height <= 0 || clip.Width <= 0 || clip.Height <= 0) return;
        EndLegacy();
        _frameBatch?.Flush();
        var device = GraphicsDevice;
        var viewport = device.Viewport; var scissor = device.ScissorRectangle;
        var blend = device.BlendState; var depth = device.DepthStencilState; var rasterizer = device.RasterizerState;
        var usage = device.PresentationParameters.RenderTargetUsage;
        int count = device.RenderTargetCount;
        if (_sceneTargets.Length != count) _sceneTargets = new RenderTargetBinding[count];
        if (count > 0) device.GetRenderTargets(_sceneTargets);
        for (int slot = 0; slot < 8; slot++)
        { _sceneTextures[slot] = device.Textures[slot]; _sceneSamplers[slot] = device.SamplerStates[slot]; }
        _drawingScene = true;
        try
        {
            // Nested scene passes must retain UI pixels already written to the backbuffer.
            if (count == 0) device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            device.Viewport = new Viewport(bounds);
            device.ScissorRectangle = clip;
            device.RasterizerState = _sceneRasterizer;
            render(new SceneViewportDrawContext(device, bounds, clip, _opacity, GameTime));
            if (compositeBackground is { } background)
            {
                if (background.A != 255) throw new ArgumentException("The composite backdrop must be opaque.", nameof(compositeBackground));
                device.Viewport = new Viewport(bounds);
                device.ScissorRectangle = clip;
                _sceneCompositeBatch ??= new SpriteBatch(device);
                _sceneCompositeBatch.Begin(SpriteSortMode.Immediate, _sceneCompositeBlend, SamplerState.PointClamp,
                    DepthStencilState.None, _sceneRasterizer);
                try { _sceneCompositeBatch.Draw(_white, new Rectangle(0, 0, bounds.Width, bounds.Height), background); }
                finally { _sceneCompositeBatch.End(); }
            }
        }
        finally
        {
            try
            {
                int current = device.RenderTargetCount;
                bool changed = current != count;
                if (!changed && current > 0)
                {
                    if (_sceneCurrentTargets.Length != current) _sceneCurrentTargets = new RenderTargetBinding[current];
                    device.GetRenderTargets(_sceneCurrentTargets);
                    for (int i = 0; i < current; i++)
                        if (!ReferenceEquals(_sceneCurrentTargets[i].RenderTarget, _sceneTargets[i].RenderTarget) ||
                            _sceneCurrentTargets[i].ArraySlice != _sceneTargets[i].ArraySlice) { changed = true; break; }
                }
                if (changed) device.SetRenderTargets(_sceneTargets);
                device.PresentationParameters.RenderTargetUsage = usage;
                device.Viewport = viewport; device.ScissorRectangle = scissor;
                device.BlendState = blend; device.DepthStencilState = depth; device.RasterizerState = rasterizer;
                for (int slot = 0; slot < 8; slot++)
                { device.Textures[slot] = _sceneTextures[slot]; device.SamplerStates[slot] = _sceneSamplers[slot]; }
            }
            finally
            {
                _drawingScene = false;
                Array.Clear(_sceneTextures); Array.Clear(_sceneTargets); Array.Clear(_sceneCurrentTargets);
            }
        }
    }

    internal static Rectangle ToPixelClip(Bounds2 clip, Rectangle viewport, float scale)
    {
        int left = (int)Math.Clamp(Math.Floor(viewport.X + (double)clip.Left * scale), viewport.Left, (long)viewport.X + viewport.Width);
        int top = (int)Math.Clamp(Math.Floor(viewport.Y + (double)clip.Top * scale), viewport.Top, (long)viewport.Y + viewport.Height);
        int right = (int)Math.Clamp(Math.Ceiling(viewport.X + (double)clip.Right * scale), left, (long)viewport.X + viewport.Width);
        int bottom = (int)Math.Clamp(Math.Ceiling(viewport.Y + (double)clip.Bottom * scale), top, (long)viewport.Y + viewport.Height);
        return new Rectangle(left, top, right - left, bottom - top);
    }

    internal void End()
    {
        if (!_active)
            return;
        try
        {
            try
            {
                EndLegacy();
            }
            finally
            {
                _frameBatch?.End();
            }
        }
        finally
        {
            _active = false;
            GraphicsDevice.Viewport = _previousViewport;
            GraphicsDevice.ScissorRectangle = _previousScissor;
            GraphicsDevice.BlendState = _previousBlend!;
            GraphicsDevice.DepthStencilState = _previousDepth!;
            GraphicsDevice.RasterizerState = _previousRasterizer!;
            GraphicsDevice.SamplerStates[0] = _previousSampler!;
        }
    }

    private void EnsureDrawing()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_active)
            throw new InvalidOperationException("Drawing is only valid inside Control.OnDraw.");
    }

    /// <summary>
    /// Releases owned graphics resources outside a draw call.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_active)
            throw new InvalidOperationException("Cannot dispose during drawing.");
        _batch?.Dispose();
        _frameBatch?.Dispose();
        _white.Dispose();
        _rasterizer.Dispose();
        _sceneRasterizer.Dispose();
        _sceneCompositeBatch?.Dispose();
        _sceneCompositeBlend.Dispose();
        _disposed = true;
    }
}
