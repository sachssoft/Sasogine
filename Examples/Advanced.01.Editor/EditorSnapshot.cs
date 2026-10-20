namespace Sachssoft.Engine.Controls.Examples;
/// <summary>One complete edit; a drag is recorded only on release.</summary>
internal sealed record EditorSnapshot(EditorObject[] Objects, int? Selection);
