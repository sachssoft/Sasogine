using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

public struct Thickness
{
    public static readonly Thickness Zero = new Thickness();

    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    [Browsable(false)]
    [XmlIgnore]
    public int Width => Left + Right;

    [Browsable(false)]
    [XmlIgnore]
    public int Height => Top + Bottom;

    [Browsable(false)]
    [XmlIgnore]
    public int Horizontal => Width;  // oder Left + Right

    [Browsable(false)]
    [XmlIgnore]
    public int Vertical => Height;  // oder Top + Bottom

    [Browsable(false)]
    [XmlIgnore]
    public bool SameSize => (Left == Top && Top == Right && Right == Bottom);

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Thickness(int horizontalValue, int verticalValue)
        : this(horizontalValue, verticalValue, horizontalValue, verticalValue)
    {
    }

    public Thickness(int value)
        : this(value, value, value, value)
    {
    }

    public override string ToString()
    {
        if (SameSize)
            return Left.ToString();

        if (Left == Right && Top == Bottom)
            return $"{Left}, {Top}";

        return $"{Left}, {Top}, {Right}, {Bottom}";
    }

    public static Thickness FromString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return Zero;

        var parts = s.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
            throw new ArgumentException($"Could not convert string '{s}' to Thickness");

        if (parts.Length == 1)
            return new Thickness(int.Parse(parts[0]));

        if (parts.Length == 2)
            return new Thickness(int.Parse(parts[0]), int.Parse(parts[1]));

        return new Thickness(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
    }

    public override bool Equals(object obj)
    {
        return obj is Thickness thickness &&
               Left == thickness.Left &&
               Right == thickness.Right &&
               Top == thickness.Top &&
               Bottom == thickness.Bottom;
    }

    public override int GetHashCode()
    {
        var hashCode = 551583723;
        hashCode = hashCode * -1521134295 + Left.GetHashCode();
        hashCode = hashCode * -1521134295 + Right.GetHashCode();
        hashCode = hashCode * -1521134295 + Top.GetHashCode();
        hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(Thickness a, Thickness b) => a.Equals(b);

    public static bool operator !=(Thickness a, Thickness b) => !(a == b);

    public static Rectangle operator -(Rectangle a, Thickness b)
    {
        var result = a;
        result.X += b.Left;
        result.Y += b.Top;

        result.Width -= b.Width;
        if (result.Width < 0) result.Width = 0;

        result.Height -= b.Height;
        if (result.Height < 0) result.Height = 0;

        return result;
    }
}
