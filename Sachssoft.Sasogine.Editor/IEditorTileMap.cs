using Microsoft.Xna.Framework;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Tiling;

namespace Sachssoft.Sasogine.Editor;

public interface IEditorTileMap : ITileMap
{
    void Update(GameFrameContext context);
    
    void Draw(TileRenderer renderer, TilePrimitive primitive, GameFrameContext context, TileDrawingOptions options, Rectangle? view_bounds = null);
    
    void Initialize();
    
    void Clear();

    ITileMap Resize(int new_columns, int new_rows);

}
