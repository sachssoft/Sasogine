
namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public interface ITreeViewNode
{
    int ChildNodesCount { get; }

    TreeViewNode AddSubNode(Widget content);
    TreeViewNode GetSubNode(int index);

    void RemoveAllSubNodes();
}
