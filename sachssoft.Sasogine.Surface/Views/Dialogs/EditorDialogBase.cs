using sachssoft.Sasogine.Surface;
using sachssoft.Sasogine.Surface.Forms;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using sachssoft.Sasogine.Views;

namespace sachssoft.Sasogine.Views.Dialogs;

public abstract class EditorDialogBase : Dialog
{
    private ViewBase _view;
    private ScrollViewer _scroll_viewer;
    private OptionListPanel _option_list;

    public EditorDialogBase(ViewBase view)
    {
        _view = view;

        _scroll_viewer = new ScrollViewer();
        _option_list = new OptionListPanel();

        ButtonConfirm.Width = 100;
        ButtonConfirm.Content.HorizontalAlignment = HorizontalAlignment.Center;
        ButtonCancel.Width = 100;
        ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

        _scroll_viewer.Content = _option_list;
        Content = _scroll_viewer;
    }

    protected ViewBase View => _view;

    protected OptionListPanel OptionList => _option_list;
}
