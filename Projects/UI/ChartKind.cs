namespace Sachssoft.Engine.Controls;
/// <summary>Chooses the geometry used to display chart samples.</summary>
public enum ChartKind
{
    /// <summary>Connect adjacent samples.</summary>
    Line,
    /// <summary>Display vertical columns from the zero baseline.</summary>
    Bar,
    /// <summary>Display horizontal steps between samples.</summary>
    Step
}
