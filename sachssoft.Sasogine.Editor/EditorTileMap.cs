using Microsoft.Xna.Framework;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Editor.Tiling;
using sachssoft.Sasogine.Tiling.Stacked;

namespace sachssoft.Sasogine.Editor;

public class EditorTileMap : EditorTileMapBase<EditorTileInstance>
{
    public EditorTileMap(ITileElementRegistry element_registry, TileLayerRegistry layer_registry, int rows, int columns) 
        : base(element_registry, layer_registry, rows, columns)
    {
    }
}
