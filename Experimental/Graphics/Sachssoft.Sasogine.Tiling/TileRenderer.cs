using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Sasogine.Tiling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Graphics.Rendering;

public sealed class TileRenderer : RendererBase
{
    private CameraBase? _camera;
    private BoundingBox _screen_bounds;
    private Vector2 _tile_size;
    private DrawBatchMode _draw_mode = DrawBatchMode.Immediate;

    private readonly List<DrawRequest> _deferred_draws = new();
    private readonly object _lock = new();

    private record struct DrawRequest(TilePrimitive primitive, Texture2D texture, Coordinate coordinate, TileDrawingOptions options);

    public TileRenderer(RuntimeBase runtime, Vector2? tile_size = null, DrawBatchMode mode = DrawBatchMode.Immediate)
        : this(runtime.Camera!, runtime.Effect!, tile_size, mode)
    {
    }

    public TileRenderer(CameraBase camera, IEffectAdapter effect, Vector2? tile_size = null, DrawBatchMode mode = DrawBatchMode.Immediate)
        : base(IGameApplication.Current.GraphicsDevice, effect, camera)
    {
        _tile_size = tile_size ?? Vector2.One;
        _draw_mode = mode;

        if (camera is Camera2D cam2d)
        {
            _screen_bounds = cam2d.GetScreenBoundingBox();
        }
    }

    public Vector2 TileSize
    {
        get => _tile_size;
        set => _tile_size = value;
    }

    public DrawBatchMode DrawMode
    {
        get => _draw_mode;
        private set => _draw_mode = value;
    }

    public CameraBase Camera => _camera!;

    protected override void OnInitialize(object[] args)
    {
        _camera = (CameraBase)args[0];

        BlendState = BlendState.NonPremultiplied;
        SamplerState = SamplerState.PointClamp;
        DepthStencil = DepthStencilState.Default;

        Rasterizer.MultiSampleAntiAlias = true;

        Effect.Projection = _camera.Projection;
        Effect.View = _camera.View;
        Effect.World = _camera.World;
    }

    protected override void OnUninitialize()
    {
        Effect.Texture = null;
        _deferred_draws.Clear();
    }

    public void SetTransform(Matrix matrix)
    {
        Effect.World = _camera!.World * matrix;
    }

    public Coordinate GetVisibleTileMinimum(Matrix? transform = null)
    {
        var min = _screen_bounds.Min;

        if (transform.HasValue)
            min = Vector3.Transform(min, Matrix.Invert(transform.Value));

        int tile_x = (int)Math.Floor(min.X / _tile_size.X);
        int tile_y = (int)Math.Floor(min.Y / _tile_size.Y);

        return new Coordinate(tile_x, tile_y);
    }

    public Coordinate GetVisibleTileMaximum(Matrix? transform = null)
    {
        var max = _screen_bounds.Max;

        if (transform.HasValue)
            max = Vector3.Transform(max, Matrix.Invert(transform.Value));

        int tile_x = (int)Math.Ceiling(max.X / _tile_size.X);
        int tile_y = (int)Math.Ceiling(max.Y / _tile_size.Y);

        return new Coordinate(tile_x, tile_y);
    }

    public void DrawTile(TilePrimitive primitive, Texture2D texture, Coordinate coordinate, TileDrawingOptions? options = null)
    {
        options ??= new TileDrawingOptions();
        var optionsCopy = options.Clone(); // Clone oder Copy-Methode, muss in TileDrawingOptions implementiert sein

        if (_draw_mode == DrawBatchMode.Immediate)
        {
            DrawTileInternal(primitive, texture, coordinate, optionsCopy);
        }
        else
        {
            lock (_lock)
            {
                _deferred_draws.Add(new DrawRequest(primitive, texture, coordinate, optionsCopy));
            }
        }
    }

    private void DrawTileInternal(TilePrimitive primitive, Texture2D texture, Coordinate coordinate, TileDrawingOptions options)
    {
        var transform =
            Matrix.CreateTranslation(-options.Origin.X, -options.Origin.Y, options.LayerDepth) *
            Matrix.CreateScale(_tile_size.X * options.Scale.X, _tile_size.Y * options.Scale.Y, 1f) *
            Matrix.CreateRotationZ(options.Rotation) *
            Matrix.CreateTranslation(_tile_size.X * coordinate.X + options.Offset.X, _tile_size.Y * coordinate.Y + options.Offset.Y, options.LayerDepth);

        if (options.TransformMatrix is Matrix user_transform)
            transform *= user_transform;

        var default_color = Effect.Color;
        var default_opacity = Effect.Opacity;
        Effect.Color = options.Color;
        Effect.Opacity = options.Opacity;

        primitive.Draw(texture, Effect, Camera, transform);

        Effect.Color = default_color;
        Effect.Opacity = default_opacity;
    }

    public override void Flush()
    {
        List<DrawRequest> items_copy;

        lock (_lock)
        {
            if (_draw_mode == DrawBatchMode.Immediate || _deferred_draws.Count == 0)
                return;

            items_copy = new List<DrawRequest>(_deferred_draws);
            _deferred_draws.Clear();
        }

        IEnumerable<DrawRequest> sorted = _draw_mode switch
        {
            DrawBatchMode.BackToFront => items_copy.OrderBy(d => d.options.LayerDepth),
            DrawBatchMode.FrontToBack => items_copy.OrderByDescending(d => d.options.LayerDepth),
            _ => items_copy
        };

        foreach (var request in sorted)
        {
            DrawTileInternal(request.primitive, request.texture, request.coordinate, request.options);
        }
    }


    protected override void OnRenderCompleted()
    {
        Flush();
    }
}
