using Microsoft.Xna.Framework;
using sachssoft.Graphics.Primitives;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Graphics.Renderer;
using sachssoft.Sasogine.Tiling;
using System;

namespace sachssoft.Sasogine.Editor;

public interface IEditorTileMap : ITileMap
{
    void Update(GameContext context);
    
    void Draw(TileRenderer renderer, TilePrimitive primitive, GameContext context, TileDrawingOptions options, Rectangle? view_bounds = null);
    
    void Initialize();
    
    void Clear();

    ITileMap Resize(int new_columns, int new_rows);

}
