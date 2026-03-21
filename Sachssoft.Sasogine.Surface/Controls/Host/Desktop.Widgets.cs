using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        public ObservableCollection<Widget> Widgets { get; } = new ObservableCollection<Widget>();

        // Widgets that are not part of the layout and are managed manually.
        // Examples include context menus, tooltips, etc.
        //public List<Widget> TransientWidgets { get; } = new List<Widget>();

        private void WidgetsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Widget w in args.NewItems!)
                {
                    w.Desktop = this;
                    w.Initialize();
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Widget w in args.OldItems!)
                {
                    w.Uninitialize();
                    w.Desktop = null;

                    if (FocusedKeyboardWidget == w)
                        FocusedKeyboardWidget = null;
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (Widget w in LayoutChildren)
                {
                    w.Uninitialize();
                    w.Desktop = null;
                }
            }

            InvalidateLayout();
            _widgetsDirty = true;
        }
    }
}
