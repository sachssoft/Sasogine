using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public class PresenterEventArgs : EventArgs
    {
        public PresenterEventArgs(Widget? oldPresenter, Widget? newPresenter)
        {
            OldPresenter = oldPresenter;
            NewPresenter = newPresenter;
        }

        public Widget? OldPresenter { get; }

        public Widget? NewPresenter { get; }
    }
}
