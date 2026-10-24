using System.Globalization;

/// <summary>Counts formatting calls and provides a deterministic failed-conversion case.</summary>
internal sealed class HudValueProbe : IFormattable
{
    internal int Conversions;
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Conversions++;
        if (format == "fail")
            throw new FormatException("Requested test failure.");
        return 42.5.ToString(format, formatProvider ?? CultureInfo.InvariantCulture);
    }

    public override string ToString() => ToString(null, null);
}
