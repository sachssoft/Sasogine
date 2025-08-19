namespace Sachssoft.Sasogine.Tiling.Composite;

public interface ICompositeTile : IOutputTile
{
    ITileElement GetTile(int layer_index);
}
