using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Graphics.Renderer;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public class TileMapRendererContext : TileContext
{
    private TileRenderer _tile_renderer;
    private TilePrimitive _tile_primitive;

    public TileMapRendererContext(
        GameFrameContext context,
        TileRenderer renderer,
        TilePrimitive tile_primitive,
        Coordinate coordinate,
        ITileMap map,
        int layer_index)
        : base(context, coordinate, map, layer_index)
    {
        _tile_renderer = renderer;
        _tile_primitive = tile_primitive;
    }

    public void Draw(Texture2D texture, TileDrawingOptions? options = null)
    {
        _tile_renderer.DrawTile(_tile_primitive, texture, Coordinate, options);
    }

    public void Draw(Texture2D texture, Coordinate coord, TileDrawingOptions? options = null)
    {
        _tile_renderer.DrawTile(_tile_primitive, texture, coord, options);
    }
}
