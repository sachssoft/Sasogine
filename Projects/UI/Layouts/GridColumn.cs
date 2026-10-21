namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Grid column configuration.</summary>
public sealed class GridColumn : GridDimension
{
    /// <summary>Gets or sets the column length.</summary>
    public GridLength Width { get => Length; set => Length = value; }
    /// <summary>Gets or sets minimum column width.</summary>
    public float MinWidth { get => Minimum; set => Minimum = value; }
    /// <summary>Gets or sets maximum column width.</summary>
    public float MaxWidth { get => Maximum; set => Maximum = value; }
}
