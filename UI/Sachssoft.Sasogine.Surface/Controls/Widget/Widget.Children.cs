using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Widget
    {
        protected internal ObservableCollection<Widget> Children { get; }
            = new ObservableCollection<Widget>();

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args == null)
                return;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems != null)
                    {
                        for (int i = 0; i < args.NewItems.Count; i++)
                        {
                            var w = args.NewItems[i] as Widget;
                            if (w != null)
                                OnChildAdded(w);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (args.OldItems != null)
                    {
                        for (int i = 0; i < args.OldItems.Count; i++)
                        {
                            var w = args.OldItems[i] as Widget;
                            if (w != null)
                                OnChildRemoved(w);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // LayoutChildren muss existieren
                    foreach (var layoutChild in LayoutChildren)
                        OnChildRemoved(layoutChild);
                    break;
            }

            InvalidateChildren();
        }

        protected virtual void OnChildAdded(Widget w)
        {
            if (w == null)
                throw new ArgumentNullException(nameof(w));

            // Kind darf nicht sich selbst als Parent haben
            if (ReferenceEquals(w, this))
                throw new InvalidOperationException("A widget cannot parent itself.");

            // Bereits bei anderem Parent?
            if (w.Parent != null && w.Parent != this)
                throw new InvalidOperationException("Widget already has a parent.");

            // Falls bereits korrekt eingehängt ⇒ skip
            if (w.Parent == this)
                return;

            w.Desktop = Desktop;
            w.Parent = this;

            // Lifecycle aufrufen
            w.Initialize();
        }

        protected virtual void OnChildRemoved(Widget w)
        {
            if (w == null)
                return;

            // Nur entfernen, wenn dieses Widget der Parent ist
            if (w.Parent != this)
                return;

            w.Uninitialize();
            w.Desktop = null;
            w.Parent = null;
        }

        private void InvalidateChildren()
        {
            InvalidateMeasure();
            _childrenDirty = true;
        }
    }
}
