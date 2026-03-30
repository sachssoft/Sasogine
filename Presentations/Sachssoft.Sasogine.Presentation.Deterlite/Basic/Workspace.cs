using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Presentation.Deterlite.Input;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public sealed class Workspace : IDisposable, IRenderingHook, IFrameChildHostInternal
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly RasterizerState _rasterizerState;
        private readonly FrameCollection _frames;
        private readonly FrameContext _frameContext;
        private readonly InputManager _inputManager;
        private readonly WorkspaceConfiguration _configuration;

        private Bounds _displayBounds;
        private Bounds _layoutBounds;

        // Scale wird nur für das Rendern berechnet, Layout bleibt unabhängig von Auflösung
        private float _scale = 1f;
        private Insets _padding = Insets.None;

        private DebugFlags _debugFlags;
        private DebugVisuals _debugVisuals = DebugVisuals.Default;

        private IFrameHook? _renderHook;
        private IFrameHook? _frameHook;

        private IBrush? _backgroundBrush;

        private bool _autoInvalidate;
        private bool _shouldInvalidate;
        private bool _isDisposed;

        public Workspace(IGameApplication application, IRenderContext? renderContext = null, WorkspaceConfiguration? configuration = null)
        {
            if (application is not Game game)
                throw new InvalidCastException("IGameApplication must be a Game instance.");

            _configuration = configuration ?? new WorkspaceConfiguration();
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

            // initial Display / Layout Bounds
            _displayBounds = new Bounds(
                0f,
                0f,
                _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight
            );

            _layoutBounds = new Bounds(
                _padding.Left,
                _padding.Top,
                _displayBounds.Width - _padding.Horizontal,
                _displayBounds.Height - _padding.Vertical
            );

            // initial Scale basierend auf DesignSize
            _scale = MathF.Min(
                _displayBounds.Width / _configuration.DesignWidth,
                _displayBounds.Height / _configuration.DesignHeight
            );
        }

        // --------------------------
        // Properties
        // --------------------------
        public Bounds DisplayBounds => _displayBounds;
        public Bounds LayoutBounds => _layoutBounds;
        public bool ShouldInvalidate => _shouldInvalidate;
        public float Scale => _scale; // ReadOnly, nur für Render-Transformation
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
            get => null; set { }
        }

        public IBrush? BackgroundBrush
        {
            get => _backgroundBrush;
            set => _backgroundBrush = value;
        }

        public bool AutoInvalidate
        {
            get => _autoInvalidate;
            set => _autoInvalidate = value;
        }

        public DebugFlags Debug
        {
            get => _debugFlags;
            set => _debugFlags = value;
        }

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

        // --------------------------
        // Update
        // --------------------------
        public void Update(GameTime gameTime)
        {
            // Input aktualisieren
            _inputManager.Update(gameTime);

            // neue Backbuffer-Größe prüfen
            var newSize = new Vector2(
                _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight
            );

            if (_displayBounds.Size != newSize)
            {
                _displayBounds = new Bounds(0, 0, newSize.X, newSize.Y);
                _shouldInvalidate = true;

                // Scale neu berechnen basierend auf Designgröße
                float scaleX = newSize.X / _configuration.DesignWidth;
                float scaleY = newSize.Y / _configuration.DesignHeight;
                _scale = MathF.Min(scaleX, scaleY);
            }

            if (_shouldInvalidate || AutoInvalidate)
            {
                _layoutBounds = new Bounds(
                    _padding.Left,
                    _padding.Top,
                    _displayBounds.Width - _padding.Horizontal,
                    _displayBounds.Height - _padding.Vertical
                );

                var availableSize = new Vector2(
                    _displayBounds.Width - _padding.Horizontal,
                    _displayBounds.Height - _padding.Vertical
                );

                var arrangeBounds = new Bounds(
                    new Vector2(_padding.Left, _padding.Top),
                    availableSize
                );

                var frameCount = _frames.Count;
                for (int i = 0; i < frameCount; i++)
                {
                    var frame = _frames[i];

                    frame.Invalidate();
                    frame.Measure(availableSize);
                    frame.Arrange(arrangeBounds);
                }
            }
        }

        // --------------------------
        // Draw
        // --------------------------
        public void Draw(GameTime gameTime)
        {
            var render = _frameContext.Render;
            render.Begin(_rasterizerState);

            // optional Hintergrund
            _backgroundBrush?.Render(_displayBounds, render);

            // Keine Skalierung, keine Offset-Translation
            //render.PushTransform(new Transform(Vector2.Zero, Vector2.One));

            // Frames rendern (Start mit Workspace-Offset)
            var rootContainer = new Bounds(
                0,
                0,
                _displayBounds.Width,
                _displayBounds.Height
            );

            var rootBounds = new LayoutBounds(rootContainer, _padding);
            //var rootBounds = new Bounds(_padding.Left, _padding.Top,
            //    _displayBounds.Width - _padding.Horizontal,
            //    _displayBounds.Height - _padding.Vertical);

            // Frames rendern
            foreach (var frame in _frames)
                RenderFrameRecursive(frame, render, gameTime, rootBounds);

            //render.PopTransform();
            render.End();

            // Debug-Borders
            if ((_debugFlags & (DebugFlags.BoundsBorder | DebugFlags.ContentBorder)) != 0)
            {
                render.Begin(_rasterizerState);
                //render.PushTransform(new Transform(Vector2.Zero, Vector2.One));

                foreach (var frame in _frames)
                    RenderDebugBordersRecursive(frame, render, rootBounds);

                //render.PopTransform();
                render.End();
            }

            // FPS Overlay
            if ((_debugFlags & DebugFlags.FPS) != 0)
            {
                render.Begin(_rasterizerState);
                //render.PushTransform(new Transform(Vector2.Zero, Vector2.One));

                RenderFPS(render);

                //render.PopTransform();
                render.End();
            }
        }

        // --------------------------
        // Layout / Render Helper
        // --------------------------
        private void RenderFrameRecursive(FrameBase frame, IRenderContext render, GameTime gameTime, LayoutBounds parentAbsoluteBounds)
        {
            if (!frame.IsVisible) return;

            // Absolute Container-Bounds relativ zum Parent Container
            var containerBounds = new Bounds(
                parentAbsoluteBounds.Container.X + frame.Bounds.X,
                parentAbsoluteBounds.Container.Y + frame.Bounds.Y,
                frame.Bounds.Width,
                frame.Bounds.Height
            );

            var absoluteBounds = new LayoutBounds(containerBounds, frame.Padding);

            _frameContext.Update(gameTime, absoluteBounds);

            // Frame selbst rendern
            if (RenderHook != null)
                RenderHook.Call(frame, _frameContext);
            else
                frame.Render(_frameContext);

            FrameHook?.Call(frame, _frameContext);

            //// Debug-Borders
            //if ((_debugFlags & DebugFlags.BoundsBorder) != 0)
            //    RenderFrameBoundsBorder(frame, render, absoluteBounds);

            //if ((_debugFlags & DebugFlags.ContentBorder) != 0)
            //    RenderFrameContentBoundsBorder(frame, render, absoluteBounds);

            // Kinder zuerst
            foreach (var child in frame.ChildFrames.VisibleSorted)
                RenderFrameRecursive(child, render, gameTime, absoluteBounds);
        }

        private void RenderDebugBordersRecursive(FrameBase frame, IRenderContext render, LayoutBounds parentAbsoluteBounds)
        {
            if (!frame.IsVisible) return;

            // Absolute Bounds für dieses Frame
            Bounds absoluteContainer = new Bounds(
                parentAbsoluteBounds.Container.X + frame.Bounds.X,
                parentAbsoluteBounds.Container.Y + frame.Bounds.Y,
                frame.Bounds.Width,
                frame.Bounds.Height
            );

            LayoutBounds absoluteBounds = new LayoutBounds(absoluteContainer, frame.Padding);

            // Debug-Borders rendern
            if ((_debugFlags & DebugFlags.BoundsBorder) != 0)
                RenderFrameBoundsBorder(frame, render, absoluteBounds);

            if ((_debugFlags & DebugFlags.ContentBorder) != 0)
                RenderFrameContentBoundsBorder(frame, render, absoluteBounds);

            // Rekursiv Kinder
            foreach (var child in frame.ChildFrames.VisibleSorted)
                RenderDebugBordersRecursive(child, render, absoluteBounds);
        }

        private void RenderFrameBoundsBorder(FrameBase frame, IRenderContext render, LayoutBounds absoluteBounds)
        {
            render.DrawBorder(absoluteBounds.Container, _debugVisuals.BoundsBorderColor, _debugVisuals.LineThickness);
        }

        private void RenderFrameContentBoundsBorder(FrameBase frame, IRenderContext render, LayoutBounds absoluteBounds)
        {
            if (absoluteBounds.Content != absoluteBounds.Container)
                render.DrawBorder(absoluteBounds.Content, _debugVisuals.ContentBorderColor, _debugVisuals.LineThickness);
        }

        private void RenderFPS(IRenderContext render)
        {
            // render.DrawText(...); // FPS Anzeige optional
        }

        public void Invalidate() => _shouldInvalidate = true;

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
}