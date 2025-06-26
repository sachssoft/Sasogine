using sachssoft.Sasogine.Tiling;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public interface IEditorCursorAction
{
    void Execute(EditorToolCursor sender, Coordinate coordinate);

    void Complete(); // Fertig
}
