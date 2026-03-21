namespace Sachssoft.Sasogine.Tiling;

public class TileContext : GameContext
{
    public TileContext(
        GameContext context,
        Coordinate coordinate,
        ITileMap map,
        int layer_index)
    {
        Coordinate = coordinate;
        Map = map;
        LayerIndex = layer_index;
    }

    public ITileMap Map { get; }

    public int LayerIndex { get; }

    public Coordinate Coordinate { get; }
}
