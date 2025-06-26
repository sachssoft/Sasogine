namespace sachssoft.Sasogine.Tiling;

public class TileContext : GameContext
{
    public TileContext(
        GameContext context,
        Coordinate coordinate,
        ITileMap map,
        int layer_index)
        : base(context)
    {
        Coordinate = coordinate;
        Map = map;
        LayerIndex = layer_index;
    }

    public ITileMap Map { get; }

    public int LayerIndex { get; }

    public Coordinate Coordinate { get; }
}
