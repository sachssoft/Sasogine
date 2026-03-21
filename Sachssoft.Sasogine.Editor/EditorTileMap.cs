using Sachssoft.Sasogine.Editor.Tiling;
using Sachssoft.Sasogine.Tiling.Stacked;

namespace Sachssoft.Sasogine.Editor;

public class EditorTileMap : EditorTileMapBase<EditorTileInstance>
{
    public EditorTileMap(ITileElementRegistry element_registry, TileLayerRegistry layer_registry, int rows, int columns) 
        : base(element_registry, layer_registry, rows, columns)
    {
    }
}
