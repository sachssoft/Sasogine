using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Geometry;

public struct Segment
{
    public Segment()
    {

    }

    public Segment(float x1, float y1, float x2, float y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y1 = y2;
    }

    public Segment(Vector2 start, Vector2 end)
    {
        X1 = start.X;
        Y1 = start.Y;
        X2 = end.X;
        Y2 = end.Y;
    }

    public float X1
    {
        get;
        set;
    }

    public float Y1
    {
        get;
        set;
    }

    public float X2
    {
        get;
        set;
    }

    public float Y2
    {
        get;
        set;
    }

    public Vector2 Start
    {
        get => new Vector2(X1, Y1);
        set
        {
            X1 = value.X;
            Y1 = value.Y;
        }
    }

    public Vector2 End
    {
        get => new Vector2(X2, Y2);
        set
        {
            X2 = value.X;
            Y2 = value.Y;
        }
    }

    public Vector2 Distance
    {
        get => End - Start;
    }

    public float Length
    {
        get => Distance.Length();
    }

    public Vector2 NormalClockwise
    {
        get
        {
            var dx = X1 - X2;
            var dy = Y1 - Y2;
            return new Vector2(-dy, dx);
        }
    }

    public Vector2 NormalAntiClockwise
    {
        get
        {
            var dx = X1 - X2;
            var dy = Y1 - Y2;
            return new Vector2(dy, -dx);
        }
    }

    // In Radians
    public float AngleBetween
    {
        get => float.Atan2(Y2 - Y1, X2 - X1);
    }

    public static Segment Parse(string? value)
    {
        // x1 y1, x2 y2
        var values = value.Split(",");
        var start = values[0].Split(" ");
        var end = values[1].Split(" ");

        var x1 = float.Parse(start[0]);
        var y1 = float.Parse(start[1]);
        var x2 = float.Parse(end[0]);
        var y2 = float.Parse(end[1]);

        return new Segment(x1, y1, x2, y2);
    }

    public override string ToString()
    {
        return $"X1: {X1}, Y1: {Y1}, X2: {X2}, Y2: {Y2}";
    }
}
