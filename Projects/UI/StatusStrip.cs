namespace Sachssoft.Engine.Controls;
/// <summary>Hosts arbitrary content inside a status bar.</summary>
public class StatusStrip : ContentControl
{
    /// <inheritdoc/>
    protected override string DefaultStyleName => "StatusStrip";

    /// <summary>
    /// ???
    /// </summary>
    public GridLength Length { get; set; } // Nicht implementiert
}
