using Microsoft.Xna.Framework;
using sachssoft.Graphics.Primitives;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Tiling;
using System;

namespace sachssoft.Sasogine.Editor;

public interface IEditorTileMap : ITileMap, IDisposable
{
    void Update(GameContext context);
    void Draw(TileRenderer renderer, TilePrimitive primitive, GameContext context, TileDrawingOptions options, Rectangle? view_bounds = null);
    void Initialize();
    void Clear();
    //void Resize(int new_columns, int new_rows);
    //void AddRowAt(int index);
    //void RemoveRowAt(int index);
    //void AddColumnAt(int index);
    //void RemoveColumnAt(int index);
    //void AddLayerAt(int index);
    //void RemoveLayerAt(int index);

}
