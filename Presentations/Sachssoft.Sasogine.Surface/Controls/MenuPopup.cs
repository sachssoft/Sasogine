using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class MenuPopup : PopupBase, IMenuNode
    {
        private readonly VerticalStackPanel _stackPanel = new VerticalStackPanel();
        private readonly ObservableCollection<MenuItemBase> _items = new ObservableCollection<MenuItemBase>();

        private bool _isLayoutUpdated = false;

        public MenuPopup()
        {
            _items.CollectionChanged += Items_CollectionChanged;
            _stackPanel.Background = Color.Black.ToBrush();
        }

        #region Direct Properties

        public ObservableCollection<MenuItemBase> Items => _items;

        #endregion

        protected override Widget CreateContent()
        {
            return _stackPanel;
        }

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuPopup();
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MenuPopup source)
                return;

            foreach (var item in source._stackPanel.Widgets)
            {
                var cloned = item.Clone();

                if (cloned is Widget clonedWidget)
                    _stackPanel.Widgets.Add(clonedWidget);
            }
        }

        #endregion

        protected override void OnContentLayoutUpdated(EventArgs e)
        {
            base.OnContentLayoutUpdated(e);
            UpdateItemsLayout();
            _isLayoutUpdated = true;
        }

        private void UpdateItemsLayout()
        {
            foreach (var item in _items)
            {
                item.UpdateLayout(Content.ContainerBounds);
            }
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (MenuItemBase item in e.NewItems!)
                    {
                        item.Owner = this;
                        var presenter = item.Presenter;
                        _stackPanel.Widgets.Insert(e.NewStartingIndex, presenter);

                        item.AddPresenterChangedHandler(OnItemPresenterChanged);
                    }

                    if (_isLayoutUpdated)
                        UpdateItemsLayout();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (MenuItemBase item in e.OldItems!)
                    {
                        var widget = _stackPanel.Widgets.FirstOrDefault(w => ReferenceEquals(w, item.Presenter));
                        if (widget != null)
                        {
                            item.Owner = null;
                            _stackPanel.Widgets.Remove(widget);
                        }

                        item.RemovePresenterChangedHandler(OnItemPresenterChanged);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (MenuItemBase item in _items)
                    {
                        item.Owner = null;
                        item.RemovePresenterChangedHandler(OnItemPresenterChanged);
                    }

                    _stackPanel.Widgets.Clear();
                    break;
            }
        }

        private void OnItemPresenterChanged(CommandItemBase<Menu, MenuItemBase> item)
        {
            for (int i = 0; i < _stackPanel.Widgets.Count; i++)
            {
                if (ReferenceEquals(_stackPanel.Widgets[i], item.Presenter))
                    return; // schon aktuell

                if (_stackPanel.Widgets[i] == item.Presenter) // alten Presenter ersetzen
                {
                    _stackPanel.Widgets[i] = item.Presenter;
                    return;
                }
            }
        }
    }
}
