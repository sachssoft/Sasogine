using Microsoft.Xna.Framework;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Graphics.Renderer;
using Sachssoft.Sasogine.Graphics.Renderer;
using System;

namespace Sachssoft.Sasogine.Tiling.Composite;

public class CompositeTileMap<T> : ITileMap where T : CompositeTileBase
{
    private readonly T[] _tiles;
    private readonly int _columns;
    private readonly int _rows;
    private readonly int _count;
    private int _rendered_tile_count;

    public CompositeTileMap(int columns, int rows)
    {
        _columns = columns;
        _rows = rows;
        _count = columns * rows;
        _tiles = new T[_count];
    }

    public int Rows => _rows;

    public int Columns => _columns;

    public int Count => _count;

    public int RenderedTileCount => _rendered_tile_count;

    public T[] Tiles => _tiles;

    object ITileMap.this[int column, int row]
    {
        get => this[column, row];
        set => this[column, row] = (T)value;
    }

    public T this[int column, int row]
    {
        get
        {
            CheckCoordinatesInBounds(column, row);
            return _tiles[row * _columns + column];
        }
        set
        {
            CheckCoordinatesInBounds(column, row);

            int index = row * _columns + column;
            var previous = _tiles[index];

            if (previous != value)
            {
                previous?.OnLeave();
                previous?.SetMapSource(null);

                _tiles[index] = value;
                value?.SetMapSource(this);
                value?.OnEnter(new Coordinate(column, row));
            }
        }
    }

    public T this[int index]
    {
        get
        {
            CheckIndexOutOfRange(index);
            return _tiles[index];
        }
        set
        {
            CheckIndexOutOfRange(index);

            var previous = _tiles[index];

            if (previous != value)
            {
                previous?.OnLeave();
                previous?.SetMapSource(null);

                _tiles[index] = value;
                int row = index / _columns;
                int column = index % _columns;
                value?.SetMapSource(this);
                value?.OnEnter(new Coordinate(column, row));
            }
        }
    }

    private void CheckCoordinatesInBounds(int column, int row)
    {
        if (column < 0 || column >= _columns || row < 0 || row >= _rows)
            throw new IndexOutOfRangeException($"Coordinates ({column}, {row}) out of bounds (0..{_columns - 1}, 0..{_rows - 1})");
    }

    private void CheckIndexOutOfRange(int index)
    {
        if (index < 0 || index >= _count)
            throw new IndexOutOfRangeException($"Index {index} out of bounds (0..{_count - 1})");
    }

    public virtual void Update(GameContext context)
    {
    }

    public virtual void Draw(TileRenderer renderer, TilePrimitive primitive, GameContext context, TileDrawingOptions options, Rectangle? view_bounds = null)
    {
        // Klasse statt struct
        var args = new CompositeTileRenderArgs(context, renderer, primitive);

        int tile_width = (int)options.TileSize.X;
        int tile_height = (int)options.TileSize.Y;

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

        _rendered_tile_count = 0;

        for (int row = min_row; row < max_row; row++)
        {
            for (int column = min_column; column < max_column; column++)
            {
                int index = row * _columns + column;
                var tile = _tiles[index];

                if (tile != null)
                {
                    // Später (performance-kritisch)
                    //// Ref struct wird *hier* direkt erzeugt – pro Aufruf temporär
                    //var args = new CompositeTileRenderArgs(context, renderer, primitive, new Coordinate(column, row));
                    args._coordinate = new Coordinate(column, row);

                    options.Reset();
                    tile.Draw(args, options); // ← Übergabe als ref struct
                    _rendered_tile_count++;
                }
            }
        }
    }
}
