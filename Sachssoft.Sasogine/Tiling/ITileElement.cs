using Sachssoft.Graphics.Renderer;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Tiling.Stacked;

namespace Sachssoft.Sasogine.Tiling;

public unsafe interface ITileElement
{    
    void Update(GameContext context);

    void Draw(TileMapRendererContext context, TileDrawingOptions options);
}
