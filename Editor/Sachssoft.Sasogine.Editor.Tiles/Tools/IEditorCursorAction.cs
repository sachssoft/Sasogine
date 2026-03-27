using Sachssoft.Sasogine.Tiling;

namespace Sachssoft.Sasogine.Editor.Tiles.Tools;

public interface IEditorCursorAction
{
    void Execute(EditorToolCursor sender, Coordinate coordinate);

    void Complete(); // Fertig
}
