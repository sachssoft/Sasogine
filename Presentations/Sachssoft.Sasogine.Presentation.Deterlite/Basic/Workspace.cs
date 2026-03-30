using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Presentation.Deterlite.Basic;
using Sachssoft.Sasogine.Presentation.Deterlite.Input;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Deterlite;

public sealed class Workspace : IDisposable, IRenderingHook, IFrameChildHostInternal
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly RasterizerState _rasterizerState;
    private readonly FrameCollection _frames;
    private readonly FrameContext _frameContext;
    private readonly InputManager _inputManager;

    private Vector2 _displaySize;
    private Vector2 _layoutSize;

    private float _scale = 1f;
    private Insets _padding = Insets.None;

    private DebugFlags _debugFlags;
    private DebugVisuals _debugVisuals = DebugVisuals.Default;

    private IFrameHook? _renderHook;
    private IFrameHook? _frameHook;

    private bool _autoInvalidate;
    private bool _shouldInvalidate;
    private bool _isDisposed;

    public Workspace(IGameApplication application, IRenderContext? renderContext = null)
    {
        if (application is not Game game)
            throw new InvalidCastException("IGameApplication must be a Game instance.");

        _graphicsDevice = game.GraphicsDevice;

        _rasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            DepthBias = 0,
            FillMode = FillMode.Solid,
            MultiSampleAntiAlias = false,
            ScissorTestEnable = true,
            SlopeScaleDepthBias = 0
        };

        _frames = new FrameCollection(this);
        _inputManager = new InputManager();
        _frameContext = new FrameContext(
            workspace: this,
            Input: _inputManager,
            render: renderContext ?? new SpriteBatchRenderContext(_graphicsDevice)
        );

        _displaySize = new Vector2(
            _graphicsDevice.PresentationParameters.BackBufferWidth,
            _graphicsDevice.PresentationParameters.BackBufferHeight
        );

        _layoutSize = new Vector2(
            _displaySize.X - _padding.Horizontal,
            _displaySize.Y - _padding.Vertical
        );
    }

    public Vector2 DisplaySize => _displaySize;

    public Vector2 LayoutSize => _layoutSize;

    public bool ShouldInvalidate => _shouldInvalidate;

    // AutoInvalidate: Wenn true, wird die Layout-Invalidate automatisch durchgeführt,
    // wenn ein Frame ShouldInvalidate = true hat. Das passiert in der Update-Methode. Wenn false, muss Invalidate() manuell aufgerufen werden.
    public bool AutoInvalidate
    {
        get => _autoInvalidate;
        set => _autoInvalidate = value;
    }

    // Debug-Flags: Feld direkter Zugriff ist minimal schneller als Property-Getter/Setter
    public DebugFlags Debug
    {
        get => _debugFlags;
        set => _debugFlags = value;
    }

    // Debug-Visuals: Welche Farben und so für die Debug-Rendering-Methoden verwendet werden sollen.
    public DebugVisuals DebugVisuals
    {
        get => _debugVisuals;
        set => _debugVisuals = value;
    }

    public IFrameHook? RenderHook
    {
        get => _renderHook;
        set => _renderHook = value;
    }

    public IFrameHook? FrameHook
    {
        get => _frameHook;
        set => _frameHook = value;
    }

    public float Scale
    {
        get => _scale;
        set
        {
            if (value <= 0f) throw new InvalidOperationException("Scale must be > 0.");
            if (!Equals(_scale, value)) { _scale = value; _shouldInvalidate = true; }
        }
    }

    public Insets Padding
    {
        get => _padding;
        set
        {
            if (Equals(_padding, value)) return;
            _padding = value;
            _shouldInvalidate = true;
        }
    }

    public FrameCollection Frames => _frames;

    FrameCollection IFrameChildHost.ChildFrames => _frames;

    IFrameChildHost? IFrameChildHostInternal.Parent
    {
        get => null;
        set { } // Workspace hat keinen Parent
    }

    public void EnsureClientResize()
    {
        var newSize = new Vector2(
            _graphicsDevice.PresentationParameters.BackBufferWidth,
            _graphicsDevice.PresentationParameters.BackBufferHeight
        );
        if (_displaySize != newSize) { _displaySize = newSize; _shouldInvalidate = true; }
    }

    public void Update(GameTime gameTime)
    {
        // Update nur Input und Status, kein Layout!
        _inputManager.Update(gameTime);

        // Workspace-Größe ggf. prüfen
        var newSize = new Vector2(
            _graphicsDevice.PresentationParameters.BackBufferWidth,
            _graphicsDevice.PresentationParameters.BackBufferHeight
        );

        if (_displaySize != newSize)
        {
            _displaySize = newSize;
            _shouldInvalidate = true;
        }

        if (_shouldInvalidate)
        {
            _layoutSize = new Vector2(_displaySize.X - _padding.Horizontal, _displaySize.Y - _padding.Vertical);
        }
    }

    public void Draw(GameTime gameTime)
    {
        var render = _frameContext.Render;
        render.Begin(_rasterizerState);
        render.PushTransform(new Vector2(_scale, _scale));

        // ---------------------------
        // Normales Rendern: Parent + Kinder
        // ---------------------------
        foreach (var frame in _frames)
        {
            RenderFrameRecursive(frame, render, gameTime);
        }

        render.PopTransform();
        render.End();

        // ---------------------------
        // Debug-Borders global rendern
        // ---------------------------
        if ((_debugFlags & (DebugFlags.BoundsBorder | DebugFlags.ContentBorder)) != 0)
        {
            render.Begin(_rasterizerState);
            render.PushTransform(new Vector2(_scale, _scale));

            foreach (var frame in _frames)
                RenderDebugBordersRecursive(frame, render);

            render.PopTransform();
            render.End();
        }

        // FPS im Vordergrund
        if ((_debugFlags & DebugFlags.FPS) != 0)
        {
            render.Begin(_rasterizerState);
            render.PushTransform(new Vector2(_scale, _scale));

            RenderFPS(render);

            render.PopTransform();
            render.End();
        }
    }

    private void RenderFrameRecursive(FrameBase frame, IRenderContext render, GameTime gameTime)
    {
        // ---------------------------
        // Layout-Invalidate
        // ---------------------------
        if (frame.ShouldInvalidate || AutoInvalidate)
        {
            Vector2 availableSize;

            if (frame.Parent == null)
            {
                // Root-Frame: gesamte Workspace minus Padding
                availableSize = new Vector2(
                    _layoutSize.X - frame.Margin.Horizontal,
                    _layoutSize.Y - frame.Margin.Vertical
                );
            }
            else
            {
                // Kind-Frame: innerhalb des Parent ContentBounds minus Margin
                availableSize = new Vector2(
                    frame.Parent.ContentBounds.Width - frame.Margin.Horizontal,
                    frame.Parent.ContentBounds.Height - frame.Margin.Vertical
                );
            }

            frame.Measure(availableSize);

            var desiredSize = frame.DesiredSize;
            float width = LayoutMath.Clamp(desiredSize.X, frame.MinWidth, float.Min(frame.MaxWidth, availableSize.X));
            float height = LayoutMath.Clamp(desiredSize.Y, frame.MinHeight, float.Min(frame.MaxHeight, availableSize.Y));

            Bounds newBounds = new Bounds(frame.Bounds.X, frame.Bounds.Y, width, height);
            frame.Arrange(newBounds);
            frame.LayoutUpdated();
        }

        if (!frame.IsVisible) return;

        // ---------------------------
        // Zuerst Kinder rendern
        // ---------------------------
        foreach (var child in frame.ChildFrames.VisibleSorted)
        {
            RenderFrameRecursive(child, render, gameTime);
        }

        // ---------------------------
        // Dann Frame selbst rendern
        // ---------------------------
        if (RenderHook != null)
            RenderHook.Call(frame, _frameContext);
        else
            frame.Render(gameTime, _frameContext);

        FrameHook?.Call(frame, _frameContext);

        // ---------------------------
        // Debug-Rendering über allem
        // ---------------------------
        if ((_debugFlags & DebugFlags.BoundsBorder) != 0)
            RenderFrameBoundsBorder(frame, render);

        if ((_debugFlags & DebugFlags.ContentBorder) != 0)
            RenderFrameContentBoundsBorder(frame, render);

        // Optional: LayerColor, Tooltip, etc. können hier ergänzt werden
        // ...
    }

    private void RenderDebugBordersRecursive(FrameBase frame, IRenderContext render)
    {
        if ((_debugFlags & DebugFlags.BoundsBorder) != 0)
            RenderFrameBoundsBorder(frame, render);

        if ((_debugFlags & DebugFlags.ContentBorder) != 0)
            RenderFrameContentBoundsBorder(frame, render);

        foreach (var child in frame.ChildFrames.VisibleSorted)
            RenderDebugBordersRecursive(child, render);
    }

    private void RenderFrameBoundsBorder(FrameBase frame, IRenderContext render)
    {
        render.DrawBorder(frame.Bounds, _debugVisuals.BoundsBorderColor, _debugVisuals.LineThickness);
    }

    private void RenderFrameContentBoundsBorder(FrameBase frame, IRenderContext render)
    {
        if (frame.ContentBounds != frame.Bounds)
            render.DrawBorder(frame.ContentBounds, _debugVisuals.ContentBorderColor, _debugVisuals.LineThickness);
    }

    private void RenderFPS(IRenderContext render)
    {
        //render.DrawText(...);
    }

    public void Invalidate()
    {
        _shouldInvalidate = true;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var frame in _frames)
            if (frame is IDisposable disposable) disposable.Dispose();

        _frames.Clear();
        _frameContext.Render.Dispose();
    }
}