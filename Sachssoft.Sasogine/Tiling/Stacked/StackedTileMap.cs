using Sachssoft.Graphics.Primitives;
using Sachssoft.Graphics.Renderer;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using Microsoft.Xna.Framework;
using System.Buffers;
using Sachssoft.Sasogine.Tiling.Stacked;
using Sachssoft.Sasogine.Graphics.Renderer;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public unsafe class StackedTileMap<TTileInstance> : IDisposable, ITileMap
    where TTileInstance : unmanaged, ITileInstance
{
    private readonly ITileElementRegistry _element_registry;
    private readonly TileLayerRegistry _layer_registry;
    private int _rows;
    private int _columns;
    private int _layers;
    private TTileInstance* _tile_data;
    private bool _disposed = false;
    private const string NameOfTileMap = "StackedTileMap";

    public int Rows { get => _rows; protected set => _rows = value; }
    public int Columns { get => _columns; protected set => _columns = value; }
    public int LayerCount { get => _layers; protected set => _layers = value; }
    protected TTileInstance* TileData { get => _tile_data; set => _tile_data = value; }
    public ITileElementRegistry Elements => _element_registry;
    public TileLayerRegistry Layers => _layer_registry;
    protected bool IsDisposed => _disposed;
    public int SelectedLayerIndex { get; set; }

    public void CopyFrom(StackedTileMap<TTileInstance> other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        // Prüfe, ob Dimensionen unterschiedlich sind
        if (Rows != other.Rows || Columns != other.Columns || LayerCount != other.LayerCount)
        {
            Rows = other.Rows;
            Columns = other.Columns;
            LayerCount = other.LayerCount;

            int new_count = Rows * Columns * LayerCount;

            // Alten Speicher freigeben, falls vorhanden
            if (_tile_data != null)
            {
                Marshal.FreeHGlobal((nint)_tile_data);
                _tile_data = null;
            }

            // Neuen Speicher allokieren
            _tile_data = (TTileInstance*)Marshal.AllocHGlobal(new_count * sizeof(TTileInstance));
        }

        // Anzahl Elemente für Kopie berechnen
        int copy_count = Rows * Columns * LayerCount;

        // Daten kopieren
        Buffer.MemoryCopy(other._tile_data, _tile_data, copy_count * sizeof(TTileInstance), copy_count * sizeof(TTileInstance));
    }

    public StackedTileMap(ITileElementRegistry element_registry, TileLayerRegistry layer_registry, int rows, int columns)
    {
        if (rows <= 0 || columns <= 0 || layer_registry.Count <= 0)
            throw new ArgumentException("StackedTileMap dimensions must be greater than zero.");

        _layers = layer_registry.Count;
        _layer_registry = layer_registry;
        _element_registry = element_registry ?? throw new ArgumentNullException(nameof(element_registry));
        _rows = rows;
        _columns = columns;

        long total_size = (long)rows * columns * _layers * sizeof(TTileInstance);
        if (total_size > int.MaxValue)
            throw new OutOfMemoryException("StackedTileMap size exceeds supported memory limit.");

        _tile_data = (TTileInstance*)Marshal.AllocHGlobal((nint)total_size);
        Unsafe.InitBlock(_tile_data, 0, (uint)total_size);
    }

    public virtual void Initialize() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int layer, int column, int row) => layer * _rows * _columns + row * _columns + column;

    public TTileInstance[] FindTileInstances(uint identifier)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(StackedTileMap<TTileInstance>));

        int total = _rows * _columns * _layers;

        // Pooled buffer (überdimensioniert)
        TTileInstance[] buffer = ArrayPool<TTileInstance>.Shared.Rent(total);
        int count = 0;

        var span = new Span<TTileInstance>(_tile_data, total);

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].Identifier == identifier)
            {
                buffer[count++] = span[i];
            }
        }

        // Result verkleinern (kopieren)
        TTileInstance[] result = new TTileInstance[count];
        Array.Copy(buffer, result, count);

        // Buffer zurückgeben
        ArrayPool<TTileInstance>.Shared.Return(buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<TTileInstance>());

        return result;
    }


    public ref TTileInstance GetTileInstance(int layer, int column, int row)
    {
        ValidateAccess(layer, column, row);
        return ref _tile_data[GetIndex(layer, column, row)];
    }

    //IOutputTile ITileMap.GetTile(int layer, int column, int row)
    //     => GetTileInstance(layer, column, row);

    public void SetTileInstance(int layer, int column, int row, TTileInstance tile)
    {
        ValidateAccess(layer, column, row);
        _tile_data[GetIndex(layer, column, row)] = tile;
    }

    //void ITileMap.SetTile(int layer, int column, int row, IOutputTile tile)
    //    => SetTileInstance(layer, column, row, (TTileInstance)tile);

    private void ValidateAccess(int layer, int column, int row)
    {
        if (_disposed) throw new ObjectDisposedException(NameOfTileMap);
        if (layer < 0 || layer >= _layers) throw new ArgumentOutOfRangeException(nameof(layer));
        if (column < 0 || column >= _columns) throw new ArgumentOutOfRangeException(nameof(column));
        if (row < 0 || row >= _rows) throw new ArgumentOutOfRangeException(nameof(row));
    }

    public void Draw(TileRenderer renderer, TilePrimitive primitive, GameContext context, TileDrawingOptions options, Rectangle? view_bounds = null)
    {
        if (_disposed) throw new ObjectDisposedException(NameOfTileMap);

        int tile_width = (int)options.TileSize.X;
        int tile_height = (int)options.TileSize.Y;
        int layer_size = _rows * _columns;

        for (int layer = 0; layer < _layers; layer++)
            RenderLayer(renderer, primitive, layer, layer_size, context, options, view_bounds, tile_width, tile_height);

    }

    public void Update(GameContext context)
    {
        if (_disposed) throw new ObjectDisposedException(NameOfTileMap);

        int layer_size = _rows * _columns;

        for (int layer = 0; layer < _layers; layer++)
        {
            if (!_layer_registry[layer].IsVisible)
                continue;

            TTileInstance* layer_start = _tile_data + layer * layer_size;

            for (int i = 0; i < layer_size; i++)
            {
                TTileInstance* tile_data = layer_start + i;
                if (tile_data->Identifier == 0) continue;

                if (_element_registry.TryGetTile(tile_data->Identifier, out var tile))
                {
                    int row = i / _columns;
                    int column = i % _columns;
                    tile?.Update(new TileContext(context, new Coordinate(column, row), this, layer));
                }
            }
        }
    }

    private int RenderLayer(TileRenderer renderer, TilePrimitive primitive, int layer, int layer_size, GameContext context, TileDrawingOptions options, Rectangle? view_bounds, int tile_width, int tile_height)
    {
        int rendered = 0;
        TTileInstance* layer_start = _tile_data + layer * layer_size;

        int min_column = 0, max_column = _columns;
        int min_row = 0, max_row = _rows;

        if (view_bounds.HasValue)
        {
            Rectangle vb = view_bounds.Value;
            min_column = int.Clamp(vb.Left / tile_width, 0, _columns);
            max_column = int.Clamp((vb.Right + tile_width - 1) / tile_width, 0, _columns);
            min_row = int.Clamp(vb.Top / tile_height, 0, _rows);
            max_row = int.Clamp((vb.Bottom + tile_height - 1) / tile_height, 0, _rows);
        }

        for (int row = min_row; row < max_row; row++)
        {
            for (int col = min_column; col < max_column; col++)
            {
                int i = row * _columns + col;
                TTileInstance* tile_data = layer_start + i;
                if (tile_data->Identifier == 0) continue;

                if (_element_registry.TryGetTile(tile_data->Identifier, out var tile))
                {
                    tile?.Draw(new TileMapRendererContext(context, renderer, primitive, new Coordinate(col, row), this, layer), options);
                }

                rendered++;
            }
        }

        return rendered;
    }

    public Dictionary<int, int> CountTileTypes()
    {
        var counts = new Dictionary<int, int>();
        int total = _rows * _columns * _layers;
        for (int i = 0; i < total; i++)
        {
            int id = (int)_tile_data[i].Identifier;
            if (id == 0) continue;
            if (counts.ContainsKey(id))
                counts[id]++;
            else
                counts[id] = 1;
        }
        return counts;
    }

    public void SerializeBinary(System.IO.Stream stream)
    {
        using BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(_rows);
        writer.Write(_columns);
        writer.Write(_layers);

        int total = _rows * _columns * _layers;
        Span<byte> buffer = new byte[sizeof(TTileInstance)];

        for (int i = 0; i < total; i++)
        {
            fixed (byte* ptr = buffer)
            {
                Unsafe.CopyBlock(ptr, (byte*)&_tile_data[i], (uint)sizeof(TTileInstance));
                writer.Write(buffer);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_tile_data != null)
            {
                Marshal.FreeHGlobal((nint)_tile_data);
                _tile_data = null;
            }
            _disposed = true;
        }
    }

    ~StackedTileMap()
    {
        Dispose(false);
    }

    public Span<TTileInstance> Tiles
    {
        get
        {
            if (_disposed) throw new ObjectDisposedException(NameOfTileMap);
            return new Span<TTileInstance>(_tile_data, _rows * _columns * _layers);
        }
    }

    object ITileMap.this[int column, int row]
    {
        get => GetTileInstance(SelectedLayerIndex, column, row);
        set => SetTileInstance(SelectedLayerIndex, column, row, (TTileInstance)value);
    }

    public Span<TTileInstance> GetLayer(int layer)
    {
        ValidateAccess(layer, 0, 0);
        int offset = layer * _rows * _columns;
        return new Span<TTileInstance>(_tile_data + offset, _rows * _columns);
    }

    public Span<TTileInstance> GetRow(int layer, int row)
    {
        ValidateAccess(layer, 0, row);
        int offset = GetIndex(layer, row, 0);
        return new Span<TTileInstance>(_tile_data + offset, _columns);
    }

    public TTileInstance[] GetColumn(int layer, int col)
    {
        ValidateAccess(layer, 0, col);
        var column = new TTileInstance[_rows];
        for (int row = 0; row < _rows; row++)
        {
            column[row] = GetTileInstance(layer, col, row);
        }
        return column;
    }
}
