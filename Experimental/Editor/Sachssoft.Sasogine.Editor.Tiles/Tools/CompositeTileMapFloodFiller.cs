using Sachssoft.Sasogine.Tiling.Composite;
using System.Collections.Generic;
using System;
using Sachssoft.Sasogine.Tiling;
using Sachssoft.Sasogine.Editor.Tiles.Tools;

public delegate object? GetTileValueFloodFillerDelegate(CompositeTileBase tile);
public delegate void ReplaceTileFloodFillerDelegate(CompositeTileBase tile, object? target_value);
public delegate bool TileValueEqualsFloodFillerDelegate(object? a, object? b);

public class CompositeTileMapFloodFiller<T> : ITileMapFloodFiller where T : CompositeTileBase
{
    private readonly CompositeTileMap<T> _map;
    private readonly GetTileValueFloodFillerDelegate _getter;
    private readonly TileValueEqualsFloodFillerDelegate _value_equals;
    private readonly ReplaceTileFloodFillerDelegate _replace;

    public CompositeTileMapFloodFiller(
        CompositeTileMap<T> map,
        GetTileValueFloodFillerDelegate getter,
        TileValueEqualsFloodFillerDelegate valueEquals,
        ReplaceTileFloodFillerDelegate replace)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _value_equals = valueEquals ?? throw new ArgumentNullException(nameof(valueEquals));
        _replace = replace ?? throw new ArgumentNullException(nameof(replace));
    }

    public CompositeTileMapFloodFiller(
        CompositeTileMap<T> map,
        GetTileValueFloodFillerDelegate getter,
        ReplaceTileFloodFillerDelegate replace)
        : this(map, getter, (a, b) => a?.GetType() == b?.GetType(), replace)
    {
    }

    public object? Target { get; set; }

    public void Fill(Coordinate position)
    {
        if (Target == null) throw new InvalidOperationException("Target must be set before calling Fill.");

        if (position.IsOutOfBorder(_map.Columns, _map.Rows))
            return;

        var original_tile = _map[position.X, position.Y];
        if (original_tile == null)
            return;

        var original_value = _getter(original_tile);
        var target_value = Target;

        if (_value_equals(original_value, target_value))
            return;

        var visited = new HashSet<Coordinate>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(position);
        visited.Add(position);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.IsOutOfBorder(_map.Columns, _map.Rows))
                continue;

            var current_tile = _map[current.X, current.Y];
            if (current_tile == null)
                continue;

            var current_value = _getter(current_tile);

            if (!_value_equals(current_value, original_value))
                continue;

            _replace(current_tile, target_value);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && !neighbor.IsOutOfBorder(_map.Columns, _map.Rows))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
    }

    private static IEnumerable<Coordinate> GetNeighbors(Coordinate c)
    {
        yield return new Coordinate(c.X + 1, c.Y);
        yield return new Coordinate(c.X - 1, c.Y);
        yield return new Coordinate(c.X, c.Y + 1);
        yield return new Coordinate(c.X, c.Y - 1);
    }
}
