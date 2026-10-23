namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>
/// Provides a compile-time generic identity without Type objects or reflection.
/// </summary>
internal static class InspectorEditorKey<T>
{
    internal static readonly object Value = new();
}
