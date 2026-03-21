using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public static class TileMapValidator
{
    public static unsafe bool IsSizeAllowed<TTileInstance>(int rows, int columns, int layers, out TileMapSizeErrors error)
        where TTileInstance : unmanaged, ITileInstance
    {
        int size = sizeof(TTileInstance);
        error = TileMapSizeErrors.None;

        if (rows <= 0)
        {
            error = TileMapSizeErrors.ZeroRows;
            return false;
        }

        if (columns <= 0)
        {
            error = TileMapSizeErrors.ZeroColumns;
            return false;
        }

        if (layers <= 0)
        {
            error = TileMapSizeErrors.ZeroLayers;
            return false;
        }

        if (size <= 0)
        {
            error = TileMapSizeErrors.InvalidInstanceSize;
            return false;
        }

        // rows * columns
        long rc = (long)rows * columns;
        if (rc > long.MaxValue / layers)
        {
            error = TileMapSizeErrors.LayersTooLarge;
            return false; // Multiplikation mit layers würde overflowen
        }

        long rcl = rc * layers;
        if (rcl > long.MaxValue / size)
        {
            error = TileMapSizeErrors.SizeOverflow;
            return false; // Multiplikation mit Größe des TileEntry würde overflowen
        }

        long total_size = rcl * size;

        if (total_size > int.MaxValue)
        {
            error = TileMapSizeErrors.TotalSizeTooLarge;
            return false; // Gesamtgröße zu groß für AllocHGlobal
        }

        return true;
    }

    public static unsafe Point GetMaxDimensions<TTileInstance>(int layers)
        where TTileInstance : unmanaged, ITileInstance
    {
        int size = sizeof(TTileInstance);
        if (layers <= 0) throw new ArgumentOutOfRangeException(nameof(layers));

        long max_cells = int.MaxValue / size;
        if (max_cells == 0)
            return Point.Zero; // (0,0)

        long max_cells_per_layer = max_cells / layers;
        if (max_cells_per_layer == 0)
            return Point.Zero;

        float max_dim_double = float.Sqrt(max_cells_per_layer);
        if (max_dim_double > int.MaxValue)
            return new Point(int.MaxValue, int.MaxValue);

        int max_dim = (int)max_dim_double;

        return new Point(max_dim, max_dim);
    }
}
