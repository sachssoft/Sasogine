namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Grid row configuration.</summary>
public sealed class RowTrack : GridTrack
{
    /// <summary>Gets or sets the row length.</summary>
    public GridLength Height { get => Length; set => Length = value; }
    /// <summary>Gets or sets minimum row height.</summary>
    public float MinHeight { get => Minimum; set => Minimum = value; }
    /// <summary>Gets or sets maximum row height.</summary>
    public float MaxHeight { get => Maximum; set => Maximum = value; }
}
