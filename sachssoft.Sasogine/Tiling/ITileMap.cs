using System.Runtime.CompilerServices;

namespace sachssoft.Sasogine.Tiling;

public interface ITileMap
{
    int Rows { get; }

    int Columns { get; }

    object this[int column, int row] { get; set; }

    //IOutputTile this[int layer, int column, int row] { get; set; }

    //int LayerCount { get; }

    //IOutputTile GetTile(int layer, int column, int row);

    //void SetTile(int layer, int column, int row, IOutputTile tile);
}
