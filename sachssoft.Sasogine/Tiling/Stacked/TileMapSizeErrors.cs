namespace sachssoft.Sasogine.Tiling.Stacked;

public enum TileMapSizeErrors
{
    None = 0,
    ZeroRows,
    ZeroColumns,
    ZeroLayers,
    InvalidInstanceSize,
    LayersTooLarge,
    SizeOverflow,
    TotalSizeTooLarge
}
