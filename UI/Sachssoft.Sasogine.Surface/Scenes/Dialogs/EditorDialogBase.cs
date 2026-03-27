using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Views.Dialogs;

public abstract class EditorDialogBase : Dialog
{
    private SceneBase _scene;
    private ScrollViewer _scroll_viewer;

    public EditorDialogBase(SceneBase scene)
    {
        _scene = scene;

        _scroll_viewer = new ScrollViewer();


        Content = _scroll_viewer;
    }

    protected SceneBase View => _scene;
}
