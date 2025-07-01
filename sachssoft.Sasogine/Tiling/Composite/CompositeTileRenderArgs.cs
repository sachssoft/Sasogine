using Microsoft.Xna.Framework.Graphics;
using sachssoft.Graphics.Primitives;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine;
using sachssoft.Sasogine.Tiling.Composite;
using sachssoft.Sasogine.Graphics.Renderer;

// Später (Performance-Kritisch) --> ref

public /*readonly ref*/ class CompositeTileRenderArgs
{
    public readonly GameContext Context;
    public readonly TileRenderer Renderer;
    public readonly TilePrimitive Primitive;
    internal Coordinate _coordinate;

    public Coordinate Coordinate => _coordinate;

    internal CompositeTileRenderArgs(GameContext context, TileRenderer renderer, TilePrimitive primitive)
    {
        Context = context;
        Renderer = renderer;
        Primitive = primitive;
    }

    public void Draw(Texture2D texture, TileDrawingOptions? options = null)
    {
        Renderer.DrawTile(Primitive, texture, _coordinate, options);
    }
}
