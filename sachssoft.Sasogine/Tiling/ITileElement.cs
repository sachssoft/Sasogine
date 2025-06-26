using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Tiling.Stacked;

namespace sachssoft.Sasogine.Tiling;

public unsafe interface ITileElement
{    
    void Update(GameContext context);

    void Draw(TileMapRendererContext context, TileDrawingOptions options);
}
