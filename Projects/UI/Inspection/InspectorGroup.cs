namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>An expander containing property fields and further nested definition groups.</summary>
public sealed class InspectorGroup : Expander
{
    /// <summary>Creates a collapsible inspector group with an arbitrary title.</summary>
    public InspectorGroup(string title, InspectorEditorRegistry? editors = null)
    {
        HeaderContent = new TextBlock
        {
            Text = title
        };
        Fields = new InspectorView(editors)
        {
            Padding = new Insets2(10),
            Background = Color.Transparent
        };
        Content = Fields;
    }

    /// <summary>Gets the fields belonging to this category or nested definition.</summary>
    public InspectorView Fields { get; }
}
