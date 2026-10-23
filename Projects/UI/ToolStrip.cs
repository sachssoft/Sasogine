namespace Sachssoft.Engine.Controls;
/// <summary>
/// Groups arbitrary controls inside a toolbar. An oversized group can also wrap its own controls.
/// </summary>
public class ToolStrip : WrapPanel
{
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToolStrip";
}
