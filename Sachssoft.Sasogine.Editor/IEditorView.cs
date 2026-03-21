using Sachssoft.Sasogine.Editor.Tiling.Tools;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Editor;

public interface IEditorView
{
    IEnumerable<EditorToolCursor> Tools
    {
        get;
    }

    IEnumerable<EditorToolMetadata> ToolMetadata
    {
        get;
    }
}
