using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Graphics.Renderer;
using Sachssoft.Sasogine.Tiling;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Tiling.Composite;
using Sachssoft.Sasogine.Graphics.Renderer;

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

    public void Draw(Texture2D texture, Coordinate coordinate, TileDrawingOptions? options = null)
    {
        Renderer.DrawTile(Primitive, texture, coordinate, options);
    }
}
