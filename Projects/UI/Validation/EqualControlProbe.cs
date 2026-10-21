using Sachssoft.Engine.Controls;

internal sealed class EqualControlProbe : Control
{
    public override bool Equals(object? other) => other is EqualControlProbe;
    public override int GetHashCode() => 0;
}
