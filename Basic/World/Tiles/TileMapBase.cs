using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.World.Tiles
{
    /// <summary>
    /// Represents a generic layered tile map that stores and manages tile objects.
    /// Supports multiple layers, tile access, resizing, and editing operations.
    /// </summary>
    /// <typeparam name="TTile">
    /// The type of tile object stored in the tile map.
    /// </typeparam>
    public abstract class TileMapBase<TTile>
        where TTile : class, ITileObject
    {
        private readonly TTile[][] _tiles;
        private readonly TTile _defaultTileCache;
        private readonly int _layerCount;
        private short _columns;
        private short _rows;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileMapBase{TTile}"/> class.
        /// </summary>
        /// <param name="layerCount">The number of layers in the tile map.</param>
        /// <param name="columns">The width of the tile map in tiles.</param>
        /// <param name="rows">The height of the tile map in tiles.</param>
        protected TileMapBase(int layerCount, short columns, short rows)
        {
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns));

            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows));

            if (layerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(layerCount));

            _defaultTileCache = CreateDefaultTile();

            _columns = columns;
            _rows = rows;
            _layerCount = layerCount;

            int tileCount = columns * rows;

            _tiles = new TTile[layerCount][];

            for (int i = 0; i < layerCount; i++)
            {
                _tiles[i] = new TTile[tileCount];

                Array.Fill(
                    _tiles[i],
                    CreateDefaultTile());
            }
        }

        /// <summary>
        /// Gets the number of layers contained in this tile map.
        /// </summary>
        public int LayerCount => _layerCount;

        /// <summary>
        /// Gets the number of columns in the tile map.
        /// </summary>
        public short Columns { get; }

        /// <summary>
        /// Gets the number of rows in the tile map.
        /// </summary>
        public short Rows { get; }

        /// <summary>
        /// Gets the total number of tile cells per layer.
        /// </summary>
        public int TileCount => _columns * _rows;

        /// <summary>
        /// Gets the tile object at the specified layer and tile coordinates.
        /// </summary>
        /// <param name="layerIndex">The layer index.</param>
        /// <param name="x">The horizontal tile coordinate.</param>
        /// <param name="y">The vertical tile coordinate.</param>
        /// <returns>The tile object located at the specified position.</returns>
        public TTile GetTile(int layerIndex, int x, int y)
        {
            ValidateCell(x, y);

            return _tiles[layerIndex][GetIndex(x, y)];
        }

        /// <summary>
        /// Gets the tile object at the specified layer and coordinate.
        /// </summary>
        /// <param name="layerIndex">The layer index.</param>
        /// <param name="cell">The tile coordinate.</param>
        /// <returns>The tile object located at the specified position.</returns>
        public TTile GetTile(int layerIndex, Coordinate2 cell)
            => GetTile(layerIndex, cell.X, cell.Y);

        /// <summary>
        /// Gets the tile object at the specified layer and linear tile index.
        /// </summary>
        /// <param name="layerIndex">The layer index.</param>
        /// <param name="index">The zero-based tile index.</param>
        /// <returns>The tile object located at the specified index.</returns>
        public TTile GetTile(int layerIndex, int index)
        {
            ValidateLayer(layerIndex);

            if (index < 0 || index >= TileCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _tiles[layerIndex][index];
        }

        /// <summary>
        /// Assigns a tile object to the specified layer and tile coordinates.
        /// </summary>
        /// <param name="layerIndex">The layer index.</param>
        /// <param name="tile">The tile object to assign. A default tile is created when null.</param>
        /// <param name="x">The horizontal tile coordinate.</param>
        /// <param name="y">The vertical tile coordinate.</param>
        public void SetTile(int layerIndex, TTile? tile, int x, int y)
        {
            ValidateCell(x, y);
            ValidateLayer(layerIndex);

            _tiles[layerIndex][GetIndex(x, y)] = tile ?? CreateDefaultTile();
        }

        /// <summary>
        /// Assigns a tile object to the specified layer and coordinate.
        /// </summary>
        /// <param name="layerIndex">The layer index.</param>
        /// <param name="tile">The tile object to assign.</param>
        /// <param name="cell">The tile coordinate.</param>
        public void SetTile(int layerIndex, TTile? tile, Coordinate2 cell)
            => SetTile(layerIndex, tile, cell.X, cell.Y);

        /// <summary>
        /// Removes all tile data and fills all cells with default tile objects.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                var tiles = _tiles[i];

                for (int j = 0; j < tiles.Length; j++)
                {
                    tiles[j] = CreateDefaultTile();
                }
            }
        }

        /// <summary>
        /// Resets the tile map to a new size and clears existing tile data.
        /// </summary>
        /// <param name="newColumns">The new number of columns.</param>
        /// <param name="newRows">The new number of rows.</param>
        public void Reset(short newColumns, short newRows)
        {
            if (newColumns <= 0)
                throw new ArgumentOutOfRangeException(nameof(newColumns));

            if (newRows <= 0)
                throw new ArgumentOutOfRangeException(nameof(newRows));

            _columns = newColumns;
            _rows = newRows;

            int tileCount = checked(newColumns * newRows);

            for (int i = 0; i < _tiles.Length; i++)
            {
                var tiles = new TTile[tileCount];

                for (int j = 0; j < tiles.Length; j++)
                {
                    tiles[j] = CreateDefaultTile();
                }

                _tiles[i] = tiles;
            }
        }

        /// <summary>
        /// Changes the tile map size while preserving existing tile objects.
        /// </summary>
        /// <param name="newColumns">The new number of columns.</param>
        /// <param name="newRows">The new number of rows.</param>
        /// <param name="columnsBefore">
        /// Indicates whether columns are inserted or removed before existing columns.
        /// </param>
        /// <param name="rowsBefore">
        /// Indicates whether rows are inserted or removed before existing rows.
        /// </param>
        public void ChangeSize(int newColumns, int newRows, bool columnsBefore, bool rowsBefore)
        {
            if (newColumns <= 0 || newColumns > short.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(newColumns));

            if (newRows <= 0 || newRows > short.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(newRows));


            int oldColumns = Columns;
            int oldRows = Rows;

            int newTileCount = checked(newColumns * newRows);


            for (int layer = 0; layer < LayerCount; layer++)
            {
                var oldTiles = _tiles[layer];
                var newTiles = new TTile[newTileCount];

                Array.Fill(
                    newTiles,
                    CreateDefaultTile());


                int columnDifference = newColumns - oldColumns;
                int rowDifference = newRows - oldRows;


                int sourceColumnOffset = 0;
                int sourceRowOffset = 0;

                int targetColumnOffset = 0;
                int targetRowOffset = 0;


                if (columnDifference < 0)
                {
                    if (columnsBefore)
                        sourceColumnOffset = -columnDifference;
                }
                else if (columnsBefore)
                {
                    targetColumnOffset = columnDifference;
                }


                if (rowDifference < 0)
                {
                    if (rowsBefore)
                        sourceRowOffset = -rowDifference;
                }
                else if (rowsBefore)
                {
                    targetRowOffset = rowDifference;
                }


                int copyColumns = Math.Min(oldColumns, newColumns);
                int copyRows = Math.Min(oldRows, newRows);


                for (int y = 0; y < copyRows; y++)
                {
                    for (int x = 0; x < copyColumns; x++)
                    {
                        int sourceX = x + sourceColumnOffset;
                        int sourceY = y + sourceRowOffset;

                        int targetX = x + targetColumnOffset;
                        int targetY = y + targetRowOffset;


                        newTiles[targetY * newColumns + targetX] =
                            oldTiles[sourceY * oldColumns + sourceX];
                    }
                }

                _tiles[layer] = newTiles;
            }


            _columns = (short)newColumns;
            _rows = (short)newRows;
        }

        /// <summary>
        /// Inserts an empty row into the tile map.
        /// </summary>
        /// <param name="row">The row index where the new row is inserted.</param>
        public void InsertRow(int row)
        {
            if (row < 0 || row > Rows)
                throw new ArgumentOutOfRangeException(nameof(row));

            short newRows = (short)Math.Clamp(Rows + 1, 0, short.MaxValue);
            int newSize = checked(Columns * newRows);

            for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                var oldTiles = _tiles[layerIndex];
                var newTiles = new TTile[newSize];

                Array.Fill(
                    newTiles,
                    CreateDefaultTile());

                for (int y = 0; y < Rows; y++)
                {
                    int sourceIndex = y * Columns;
                    int targetIndex = y < row
                        ? y * Columns
                        : (y + 1) * Columns;

                    Array.Copy(
                        oldTiles,
                        sourceIndex,
                        newTiles,
                        targetIndex,
                        Columns);
                }

                _tiles[layerIndex] = newTiles;
            }

            _rows = newRows;
        }

        /// <summary>
        /// Inserts an empty column into the tile map.
        /// </summary>
        /// <param name="column">The column index where the new column is inserted.</param>
        public void InsertColumn(int column)
        {
            if (column < 0 || column > Columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            short newColumns = (short)Math.Clamp(Columns + 1, 0, short.MaxValue);

            for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                var oldTiles = _tiles[layerIndex];
                var newTiles = new TTile[newColumns * Rows];

                Array.Fill(
                    newTiles,
                    CreateDefaultTile());

                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        int source = y * Columns + x;

                        int targetX = x < column ? x : x + 1;
                        int target = y * newColumns + targetX;

                        newTiles[target] = oldTiles[source];
                    }
                }

                _tiles[layerIndex] = newTiles;
            }

            _columns = newColumns;
        }

        /// <summary>
        /// Removes a row from the tile map.
        /// </summary>
        /// <param name="row">The row index to remove.</param>
        public void RemoveRow(int row)
        {
            if (Rows <= 1)
                throw new InvalidOperationException("Cannot remove the last row.");

            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row));

            short newRows = (short)Math.Clamp(Rows - 1, 0, short.MaxValue);
            int newSize = checked(Columns * newRows);

            for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                var oldTiles = _tiles[layerIndex];
                var newTiles = new TTile[newSize];

                Array.Fill(
                    newTiles,
                    CreateDefaultTile());

                int targetRow = 0;

                for (int y = 0; y < Rows; y++)
                {
                    if (y == row)
                        continue;

                    Array.Copy(
                        oldTiles,
                        y * Columns,
                        newTiles,
                        targetRow * Columns,
                        Columns);

                    targetRow++;
                }

                _tiles[layerIndex] = newTiles;
            }

            _rows = newRows;
        }

        /// <summary>
        /// Removes a column from the tile map.
        /// </summary>
        /// <param name="column">The column index to remove.</param>
        public void RemoveColumn(int column)
        {
            if (Columns <= 1)
                throw new InvalidOperationException("Cannot remove the last column.");

            if (column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            short newColumns = (short)Math.Clamp(Columns - 1, 0, short.MaxValue);

            for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++)
            {
                var oldTiles = _tiles[layerIndex];
                var newTiles = new TTile[newColumns * Rows];

                Array.Fill(
                    newTiles,
                    CreateDefaultTile());

                for (int y = 0; y < Rows; y++)
                {
                    int targetX = 0;

                    for (int x = 0; x < Columns; x++)
                    {
                        if (x == column)
                            continue;

                        newTiles[y * newColumns + targetX] =
                            oldTiles[y * Columns + x];

                        targetX++;
                    }
                }

                _tiles[layerIndex] = newTiles;
            }

            _columns = newColumns;
        }

        /// <summary>
        /// Determines whether the specified row count can be used for resizing.
        /// </summary>
        /// <param name="newRows">The requested row count.</param>
        /// <returns>
        /// <see langword="true"/> if the size is valid; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanInsertRows(short newRows)
        {
            if (newRows <= 0 || newRows > short.MaxValue)
                return false;

            long tileCount = (long)Columns * newRows;

            return tileCount <= int.MaxValue;
        }

        /// <summary>
        /// Determines whether the specified column count can be used for resizing.
        /// </summary>
        /// <param name="newColumns">The requested column count.</param>
        /// <returns>
        /// <see langword="true"/> if the size is valid; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanInsertColumns(short newColumns)
        {
            if (newColumns <= 0 || newColumns > short.MaxValue)
                return false;

            long tileCount = (long)newColumns * Rows;

            return tileCount <= int.MaxValue;
        }

        /// <summary>
        /// Determines whether the tile map can be reduced to the specified row count.
        /// </summary>
        /// <param name="newRows">The resulting row count.</param>
        /// <returns>
        /// <see langword="true"/> if rows can be removed; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanRemoveRows(short newRows)
        {
            if (newRows <= 0)
                return false;

            return newRows < Rows;
        }

        /// <summary>
        /// Determines whether the tile map can be reduced to the specified column count.
        /// </summary>
        /// <param name="newColumns">The resulting column count.</param>
        /// <returns>
        /// <see langword="true"/> if columns can be removed; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanRemoveColumns(short newColumns)
        {
            if (newColumns <= 0)
                return false;

            return newColumns < Columns;
        }

        /// <summary>
        /// Creates a new default tile object.
        /// </summary>
        /// <returns>A new default tile instance.</returns>
        protected virtual TTile CreateDefaultTile()
        {
            return (TTile)_defaultTileCache.Clone();
        }

        /// <summary>
        /// Converts tile coordinates into a zero-based linear tile index.
        /// </summary>
        /// <param name="column">The horizontal tile coordinate.</param>
        /// <param name="row">The vertical tile coordinate.</param>
        /// <returns>The linear index of the tile.</returns>
        public int GetIndex(int column, int row)
        {
            return row * Columns + column;
        }

        private void ValidateLayer(int layer)
        {
            if (layer < 0 || layer >= LayerCount)
                throw new ArgumentOutOfRangeException(nameof(layer));
        }

        private void ValidateCell(int x, int y)
        {
            if (x < 0 || x >= Columns)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= Rows)
                throw new ArgumentOutOfRangeException(nameof(y));
        }
    }
}