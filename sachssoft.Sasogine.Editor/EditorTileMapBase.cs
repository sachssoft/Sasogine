using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine.Tiling.Stacked;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace sachssoft.Sasogine.Editor;

public abstract unsafe class EditorTileMapBase<TTileInstance> : StackedTileMap<TTileInstance>, IEditorTileMap
    where TTileInstance : unmanaged, ITileInstance
{
    private const string NameOfTileMap = "EditorTileMapBase";

    public event EventHandler? DimensionChanged;

    public EditorTileMapBase(ITileElementRegistry element_registry, TileLayerRegistry layer_registry, int rows, int columns)
        : base(element_registry, layer_registry, rows, columns)
    {
    }

    public TileMatrix<IOutputTile> GetMatrix(Coordinate coordinate, int layer, Func<TTileInstance?, TTileInstance?, int>? match = null)
    {
        Func<IOutputTile?, IOutputTile?, int>? wrapped_match = null;

        if (match != null)
        {
            wrapped_match = (i1, i2) => match((TTileInstance?)i1, (TTileInstance?)i2);
        }

        return new TileMatrix<IOutputTile>(coordinate, Columns, Rows, (c) => GetTileInstance(layer, c.X, c.Y), wrapped_match);
    }

    public void Clear()
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);
        long total = (long)Rows * Columns * LayerCount;
        Unsafe.InitBlock(TileData, 0, (uint)(total * sizeof(TTileInstance)));
    }

    public void Clear(uint default_identifier, uint layer_index)
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);
        if (layer_index >= (uint)LayerCount)
            throw new ArgumentOutOfRangeException(nameof(layer_index));

        int size = Rows * Columns;
        TTileInstance* layer_start = TileData + layer_index * size;

        for (int i = 0; i < size; i++)
        {
            layer_start[i].Identifier = default_identifier;
        }
    }

    public void Clear(uint default_identifier)
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);

        int size = Rows * Columns;

        for (int layer = 0; layer < LayerCount; layer++)
        {
            TTileInstance* layer_start = TileData + layer * size;

            for (int i = 0; i < size; i++)
            {
                layer_start[i].Identifier = default_identifier;
            }
        }
    }

    private void ResizeDimension(
        bool isRowDimension,
        int index,
        bool insert)
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);

        int oldRows = Rows;
        int oldCols = Columns;

        int newRows = oldRows;
        int newCols = oldCols;

        if (isRowDimension)
            newRows = insert ? oldRows + 1 : oldRows - 1;
        else
            newCols = insert ? oldCols + 1 : oldCols - 1;

        if (newRows <= 0 || newCols <= 0)
            throw new InvalidOperationException("StackedTileMap dimensions must be positive.");

        if (index < 0 || (insert ? index > (isRowDimension ? oldRows : oldCols) : index >= (isRowDimension ? oldRows : oldCols)))
            throw new ArgumentOutOfRangeException(nameof(index));

        long newSize = (long)newRows * newCols * LayerCount * sizeof(TTileInstance);
        TTileInstance* newTiles = (TTileInstance*)Marshal.AllocHGlobal((IntPtr)newSize);
        Unsafe.InitBlock(newTiles, 0, (uint)newSize);

        int oldLayerSize = oldRows * oldCols;
        int newLayerSize = newRows * newCols;

        for (int layer = 0; layer < LayerCount; layer++)
        {
            TTileInstance* oldLayerStart = TileData + layer * oldLayerSize;
            TTileInstance* newLayerStart = newTiles + layer * newLayerSize;

            if (isRowDimension)
            {
                for (int row = 0; row < (insert ? newRows : oldRows); row++)
                {
                    if (insert)
                    {
                        if (row < index)
                        {
                            Unsafe.CopyBlock(
                                (byte*)(newLayerStart + row * newCols),
                                (byte*)(oldLayerStart + row * oldCols),
                                (uint)(oldCols * sizeof(TTileInstance))
                            );
                        }
                        else if (row == index)
                        {
                            // Neue Zeile leer
                        }
                        else
                        {
                            Unsafe.CopyBlock(
                                (byte*)(newLayerStart + row * newCols),
                                (byte*)(oldLayerStart + (row - 1) * oldCols),
                                (uint)(oldCols * sizeof(TTileInstance))
                            );
                        }
                    }
                    else
                    {
                        if (row < index)
                        {
                            Unsafe.CopyBlock(
                                (byte*)(newLayerStart + row * newCols),
                                (byte*)(oldLayerStart + row * oldCols),
                                (uint)(oldCols * sizeof(TTileInstance))
                            );
                        }
                        else
                        {
                            Unsafe.CopyBlock(
                                (byte*)(newLayerStart + row * newCols),
                                (byte*)(oldLayerStart + (row + 1) * oldCols),
                                (uint)(oldCols * sizeof(TTileInstance))
                            );
                        }
                    }
                }
            }
            else
            {
                // Spalten einfügen/entfernen
                for (int row = 0; row < oldRows; row++)
                {
                    for (int col = 0; col < (insert ? newCols : oldCols); col++)
                    {
                        if (insert)
                        {
                            if (col < index)
                            {
                                newLayerStart[row * newCols + col] = oldLayerStart[row * oldCols + col];
                            }
                            else if (col == index)
                            {
                                newLayerStart[row * newCols + col] = default;
                            }
                            else
                            {
                                newLayerStart[row * newCols + col] = oldLayerStart[row * oldCols + col - 1];
                            }
                        }
                        else
                        {
                            if (col < index)
                            {
                                newLayerStart[row * newCols + col] = oldLayerStart[row * oldCols + col];
                            }
                            else
                            {
                                newLayerStart[row * newCols + col] = oldLayerStart[row * oldCols + col + 1];
                            }
                        }
                    }
                }
            }
        }

        Marshal.FreeHGlobal((nint)TileData);
        TileData = newTiles;
        Rows = newRows;
        Columns = newCols;
        DimensionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddRowAt(int index) => ResizeDimension(true, index, true);
    public void RemoveRowAt(int index) => ResizeDimension(true, index, false);
    public void AddColumnAt(int index) => ResizeDimension(false, index, true);
    public void RemoveColumnAt(int index) => ResizeDimension(false, index, false);

    public void AddLayerAt(int index)
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);
        if (index < 0 || index > LayerCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int layerSize = Rows * Columns;
        long newSize = (long)Rows * Columns * (LayerCount + 1) * sizeof(TTileInstance);

        TTileInstance* newTiles = (TTileInstance*)Marshal.AllocHGlobal((IntPtr)newSize);
        Unsafe.InitBlock(newTiles, 0, (uint)newSize);

        // Layer vor index kopieren
        if (index > 0)
        {
            long beforeSize = (long)layerSize * index * sizeof(TTileInstance);
            Buffer.MemoryCopy(TileData, newTiles, newSize, beforeSize);
        }

        // Layer nach index kopieren
        if (index < LayerCount)
        {
            long afterSize = (long)layerSize * (LayerCount - index) * sizeof(TTileInstance);
            Buffer.MemoryCopy(TileData + index * layerSize, newTiles + (index + 1) * layerSize, newSize - ((index + 1) * layerSize * sizeof(TTileInstance)), afterSize);
        }

        Marshal.FreeHGlobal((IntPtr)TileData);
        TileData = newTiles;
        LayerCount++;
    }

    public void RemoveLayerAt(int index)
    {
        if (IsDisposed) throw new ObjectDisposedException(NameOfTileMap);
        if (LayerCount <= 1)
            throw new InvalidOperationException("At least one layer must remain.");
        if (index < 0 || index >= LayerCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        int layerSize = Rows * Columns;
        long newSize = (long)layerSize * (LayerCount - 1) * sizeof(TTileInstance);
        TTileInstance* newTiles = (TTileInstance*)Marshal.AllocHGlobal((IntPtr)newSize);
        Unsafe.InitBlock(newTiles, 0, (uint)newSize);

        // Layer vor index kopieren
        if (index > 0)
        {
            long beforeSize = (long)layerSize * index * sizeof(TTileInstance);
            Buffer.MemoryCopy(TileData, newTiles, newSize, beforeSize);
        }

        // Layer nach index kopieren (verschoben nach links)
        if (index < LayerCount - 1)
        {
            long afterSize = (long)layerSize * (LayerCount - index - 1) * sizeof(TTileInstance);
            Buffer.MemoryCopy(TileData + (index + 1) * layerSize, newTiles + index * layerSize, newSize - (index * layerSize * sizeof(TTileInstance)), afterSize);
        }

        Marshal.FreeHGlobal((nint)TileData);
        TileData = newTiles;
        LayerCount--;
    }

    public ITileMap Resize(int new_columns, int new_rows)
    {
        throw new NotImplementedException();
    }

    ~EditorTileMapBase()
    {
        if (!IsDisposed)
        {
            Marshal.FreeHGlobal((nint)TileData);
        }
    }
}
