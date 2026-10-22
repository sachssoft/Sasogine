using System;

/// <summary>Counts text conversion and simulates values whose formatting fails or changes.</summary>
internal sealed class ContentValueProbe
{
    internal string? Text = "Value";
    internal bool ThrowOnConvert;
    internal int Conversions;

    public override string? ToString()
    {
        Conversions++;
        if (ThrowOnConvert) throw new InvalidOperationException("Formatting failed.");
        return Text;
    }
}
