using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Graphics;
using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine;
using System;
using sachssoft.Graphics.Primitives;

namespace sachssoft.Graphics.Renderer;

public sealed class TileRenderer : RendererBase
{
    private CameraBase? _camera;
    private BoundingBox _screen_bounds;
    private Vector2 _tile_size;  // tile_size als Feld speichern

    public TileRenderer(RuntimeBase runtime, Vector2? tile_size = null)
        : this(runtime.Camera!, runtime.Effect!, tile_size)
    {
    }

    public TileRenderer(CameraBase camera, IEffect effect, Vector2? tile_size = null)
        : base(IMyGameApp.Current.GraphicsDevice, effect, camera)
    {
        _tile_size = tile_size ?? Vector2.One;

        if (camera is Camera2D cam2d)
        {
            _screen_bounds = cam2d.GetScreenBoundingBox();
        }
    }

    public Vector2 TileSize { get => _tile_size; set => _tile_size = value; }

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
    }

    public CameraBase Camera => _camera!;

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

        var transform = Matrix.Identity
            * Matrix.CreateTranslation(-options.Origin.X, -options.Origin.Y, options.LayerDepth)
            * Matrix.CreateScale(_tile_size.X * options.Scale.X, _tile_size.Y * options.Scale.Y, 1f)
            * Matrix.CreateRotationZ(options.Rotation)
            * Matrix.CreateTranslation(_tile_size.X * coordinate.X + options.Offset.X, _tile_size.Y * coordinate.Y + options.Offset.Y, options.LayerDepth);

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
}
