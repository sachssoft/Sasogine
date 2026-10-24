namespace Sachssoft.Engine.Controls;
/// <summary>Displays the expanded expander appearance with permanently visible content and no expansion indicator.</summary>
public class Header : GroupBox
{
    /// <summary>Creates a non-collapsible group with an expander-style header row.</summary>
    public Header() : base(new StaticHeaderPresenter())
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Header";
}
