namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>Provides non-reflective operations for one explicitly bound property.</summary>
internal interface IInspectorPropertyActions
{
    void Reset();
    void Copy(InspectorClipboard clipboard);
    bool CanPaste(InspectorClipboard clipboard);
    void Paste(InspectorClipboard clipboard);
}
