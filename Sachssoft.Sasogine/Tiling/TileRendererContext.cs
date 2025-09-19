using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Graphics.Renderer;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Tiling;

public class TileRendererContext
{
    private TileRenderer _tile_renderer;
    private TilePrimitive _tile_primitive;

    public TileRendererContext(
        GameFrameContext context,
        TileRenderer renderer,
        TilePrimitive tile_primitive)
    {
        _tile_renderer = renderer;
        _tile_primitive = tile_primitive;
    }

    public void Draw(Texture2D? texture, Coordinate coord, TileDrawingOptions? options = null)
    {
        if (texture != null)
        _tile_renderer.DrawTile(_tile_primitive, texture, coord, options);
    }
}
