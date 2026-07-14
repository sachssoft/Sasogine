using System;

namespace Sachssoft.Sasogine.Tiling;

public class TileMatrix<TTile> : ITileMatrix
{
    private readonly Func<TTile?, TTile?, int>? _match;
    private readonly Coordinate _coordinate;
    private readonly int _columns;
    private readonly int _rows;

    public TileMatrix(
        Coordinate coordinate, 
        int columns, 
        int rows,
        Func<Coordinate, TTile?> get_tile,
        Func<TTile?, TTile?, int>? match = null)
    {
        _match = match;
        _coordinate = coordinate;
        _columns = columns;
        _rows = rows;
        var x = _coordinate.X;
        var y = _coordinate.Y;

        Center = get_tile(new Coordinate(x, y));

        // Links
        if (x - 1 >= 0 && x - 1 < _columns)
            LeftNeighbor = get_tile(new Coordinate(x - 1, y));

        // Rechts
        if (x + 1 >= 0 && x + 1 < _columns)
            RightNeighbor = get_tile(new Coordinate(x + 1, y));

        // Oben
        if (y + 1 >= 0 && y + 1 < _rows)
            TopNeighbor = get_tile(new Coordinate(x, y + 1));

        // Unten
        if (y - 1 >= 0 && y - 1 < _rows)
            BottomNeighbor = get_tile(new Coordinate(x, y - 1));

        // Links Oben
        if (x - 1 >= 0 && x - 1 < _columns && y + 1 >= 0 && y + 1 < _rows)
            TopLeftNeighbor = get_tile(new Coordinate(x - 1, y + 1));

        // Rechts Oben
        if (x + 1 >= 0 && x + 1 < _columns && y + 1 >= 0 && y + 1 < _rows)
            TopRightNeighbor = get_tile(new Coordinate(x + 1, y + 1));

        // Links unten
        if (x - 1 >= 0 && x - 1 < _columns && y - 1 >= 0 && y - 1 < _rows)
            BottomLeftNeighbor = get_tile(new Coordinate(x - 1, y - 1));

        // Rechts unten
        if (x + 1 >= 0 && x + 1 < _columns && y - 1 >= 0 && y - 1 < _rows)
            BottomRightNeighbor = get_tile(new Coordinate(x + 1, y - 1));
    }

    private int IsMatch(TTile? neighbor)
    {
        if (neighbor == null && Center == null) return 1;
        if (Center == null) return 1;
        if (neighbor == null) return 0;

        //if (neighbor?.Identifier == _center_tile.Identifier)
        //    return 1;

        if (_match?.Invoke(neighbor, Center) != 0) return 1;

        return 0;
    }

    public int GetNeighborMask()
    {
        var l = IsMatch(LeftNeighbor);
        var r = IsMatch(RightNeighbor);
        var b = IsMatch(BottomNeighbor);
        var t = IsMatch(TopNeighbor);
        var tl = IsMatch(TopLeftNeighbor);
        var tr = IsMatch(TopRightNeighbor);
        var bl = IsMatch(BottomLeftNeighbor);
        var br = IsMatch(BottomRightNeighbor);

        // Binäre Konditionen
        // l r b t tl tr bl br => 0b_0000_0000

        var mask = 0;
        mask |= l << 7;
        mask |= r << 6;
        mask |= b << 5;
        mask |= t << 4;
        mask |= tl << 3;
        mask |= tr << 2;
        mask |= bl << 1;
        mask |= br << 0;

        return mask;
    }

    // Aufrecht
    public int GetOrthogonalMask()
    {
        var l = IsMatch(LeftNeighbor);
        var r = IsMatch(RightNeighbor);
        var b = IsMatch(BottomNeighbor);
        var t = IsMatch(TopNeighbor);

        // Binäre Konditionen
        // l r b t => 0000

        var mask = 0;
        mask |= l << 3;
        mask |= r << 2;
        mask |= b << 1;
        mask |= t << 0;

        return mask;
    }

    // Diagonal
    public int GetDiagonalMask()
    {
        var tl = IsMatch(TopLeftNeighbor);
        var tr = IsMatch(TopRightNeighbor);
        var bl = IsMatch(BottomLeftNeighbor);
        var br = IsMatch(BottomRightNeighbor);

        // Binäre Konditionen
        // tl tr bl br => 0000

        var mask = 0;
        mask |= tl << 3;
        mask |= tr << 2;
        mask |= bl << 1;
        mask |= br << 0;

        return mask;
    }

    public int MatchesLeft() => IsMatch(LeftNeighbor);
    public int MatchesRight() => IsMatch(RightNeighbor);
    public int MatchesTop() => IsMatch(TopNeighbor);
    public int MatchesBottom() => IsMatch(BottomNeighbor);
    public int MatchesTopLeft() => IsMatch(TopLeftNeighbor);
    public int MatchesTopRight() => IsMatch(TopRightNeighbor);
    public int MatchesBottomLeft() => IsMatch(BottomLeftNeighbor);
    public int MatchesBottomRight() => IsMatch(BottomRightNeighbor);

    // Zentriert
    public TTile? Center { get; private set; }

    // Oben
    public TTile? TopNeighbor { get; private set; }

    // Unten
    public TTile? BottomNeighbor { get; private set; }

    // Links
    public TTile? LeftNeighbor { get; private set; }

    // Rechts
    public TTile? RightNeighbor { get; private set; }

    // Oben Links
    public TTile? TopLeftNeighbor { get; private set; }

    // Oben Rechts
    public TTile? TopRightNeighbor { get; private set; }

    // Unten Links
    public TTile? BottomLeftNeighbor { get; private set; }

    // Unten Rechts
    public TTile? BottomRightNeighbor { get; private set; }
}
