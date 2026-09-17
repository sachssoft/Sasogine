using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Represents a two-dimensional grid of textures.
/// </summary>
public sealed class TextureGrid : IEnumerable<Texture2D?>
{
    private readonly Texture2D?[,] _textures;

    /// <summary>
    /// Initializes a new texture grid with the specified dimensions.
    /// </summary>
    /// <param name="columns">
    /// The number of columns.
    /// </param>
    /// <param name="rows">
    /// The number of rows.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="columns"/> or <paramref name="rows"/> is less than or
    /// equal to zero.
    /// </exception>
    public TextureGrid(
        int columns,
        int rows)
    {
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns));

        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(rows));

        Columns = columns;
        Rows = rows;

        _textures = new Texture2D?[columns, rows];
    }

    /// <summary>
    /// Gets the number of columns.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the total number of cells.
    /// </summary>
    public int Count => Columns * Rows;

    /// <summary>
    /// Gets or sets the texture at the specified grid position.
    /// </summary>
    /// <param name="column">
    /// The column index.
    /// </param>
    /// <param name="row">
    /// The row index.
    /// </param>
    /// <returns>
    /// The texture assigned to the specified cell, or
    /// <see langword="null"/> if the cell is empty.
    /// </returns>
    public Texture2D? this[int column, int row]
    {
        get
        {
            ValidatePosition(column, row);
            return _textures[column, row];
        }
        set
        {
            ValidatePosition(column, row);
            _textures[column, row] = value;
        }
    }

    /// <summary>
    /// Determines whether the specified grid position is valid.
    /// </summary>
    /// <param name="column">
    /// The column index.
    /// </param>
    /// <param name="row">
    /// The row index.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the position is inside the grid;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        int column,
        int row)
    {
        return column >= 0 &&
               column < Columns &&
               row >= 0 &&
               row < Rows;
    }

    /// <summary>
    /// Gets the texture at the specified grid position.
    /// </summary>
    public Texture2D? GetTexture(
        int column,
        int row)
    {
        return this[column, row];
    }

    /// <summary>
    /// Sets the texture at the specified grid position.
    /// </summary>
    public void SetTexture(
        int column,
        int row,
        Texture2D? texture)
    {
        this[column, row] = texture;
    }

    /// <summary>
    /// Removes the texture from the specified grid position.
    /// </summary>
    public void Clear(
        int column,
        int row)
    {
        this[column, row] = null;
    }

    /// <summary>
    /// Removes all textures from the grid.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_textures);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the textures in
    /// row-major order.
    /// </summary>
    public IEnumerator<Texture2D?> GetEnumerator()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
                yield return _textures[column, row];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void ValidatePosition(
        int column,
        int row)
    {
        if ((uint)column >= (uint)Columns)
            throw new ArgumentOutOfRangeException(nameof(column));

        if ((uint)row >= (uint)Rows)
            throw new ArgumentOutOfRangeException(nameof(row));
    }
}