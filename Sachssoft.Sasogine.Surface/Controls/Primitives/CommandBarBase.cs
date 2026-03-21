using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class CommandBarBase<TBar, TItemBase> : Widget
        where TBar : CommandBarBase<TBar, TItemBase>
        where TItemBase : CommandItemBase<TBar, TItemBase>
    {
        private StyleProperty<Orientation> _orientation = new StyleProperty<Orientation>(Visuals.Orientation.Horizontal, isUserSet: false);
        private StyleProperty<Thickness> _iconMargin = new StyleProperty<Thickness>(new Thickness(2), isUserSet: false);
        private StyleProperty<int> _iconWidth = new StyleProperty<int>(24, isUserSet: false);
        private StyleProperty<int> _iconHeight = new StyleProperty<int>(24, isUserSet: false);

        private IPresenterTemplateFactory<Widget, CommandItemBase<TBar, TItemBase>>? _itemTemplate;
        private readonly Container _container;
        private readonly ObservableCollection<TItemBase> _innerItems = new ObservableCollection<TItemBase>();
        private readonly SingleItemLayout<HorizontalWrapPanel> _layout;
        private bool _isLoaded = false;

        public CommandBarBase()
        {
            _container = CreateContainer();
            _container.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _container.VerticalAlignment = Visuals.VerticalAlignment.Stretch;

            _layout = new SingleItemLayout<HorizontalWrapPanel>(this);
            LayoutContainer = _layout;
            Children.Add(_container);

            VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Top);
            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);

            _innerItems.CollectionChanged += Items_CollectionChanged;
        }

        #region Style Properties

        public StyleProperty<Orientation> Orientation
        {
            get => _orientation;
            set
            {
                if (SetAndNotify(ref _orientation, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<Thickness> IconMargin
        {
            get => _iconMargin;
            set
            {
                if (SetAndNotify(ref _iconMargin, value))
                {
                }
            }
        }

        public StyleProperty<int> IconWidth
        {
            get => _iconWidth;
            set
            {
                if (SetAndNotify(ref _iconWidth, value))
                {
                }
            }
        }

        public StyleProperty<int> IconHeight
        {
            get => _iconHeight;
            set
            {
                if (SetAndNotify(ref _iconHeight, value))
                {
                }
            }
        }

        #endregion

        #region Direct Properties

        public ObservableCollection<TItemBase> InnerItems => _innerItems;

        public int BarSize
        {
            get
            {
                if (_orientation.Value == Visuals.Orientation.Horizontal)
                {
                    return _iconWidth.Value + _iconMargin.Value.Left + _iconMargin.Value.Right;
                }
                else
                {
                    return _iconHeight.Value + _iconMargin.Value.Top + _iconMargin.Value.Bottom;
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Orientation):
                        target.Orientation = target.Orientation.Override(value.ConvertToEnum<Orientation>());
                        break;

                    case nameof(IconMargin):
                        target.IconMargin = target.IconMargin.Override(value.ConvertTo<Thickness>());
                        break;

                    case nameof(IconWidth):
                        target.IconWidth = target.IconWidth.Override(value.ConvertTo<int>());
                        break;

                    case nameof(IconHeight):
                        target.IconHeight = target.IconHeight.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not CommandBarBase<TBar, TItemBase> source)
                return;

            Orientation = source.Orientation;
            IconMargin = source.IconMargin;
            IconWidth = source.IconWidth;
            IconHeight = source.IconHeight;
        }

        #endregion

        protected virtual Container CreateContainer()
        {
            return new HorizontalWrapPanel();
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (CommandItemBase<TBar, TItemBase> item in e.NewItems!)
                    {
                        item.Owner = (TBar?)this;
                        var presenter = item.Presenter;
                        _container.Widgets.Insert(e.NewStartingIndex, presenter);

                        item.AddPresenterChangedHandler(OnItemPresenterChanged);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (CommandItemBase<TBar, TItemBase> item in e.OldItems!)
                    {
                        var widget = _container.Widgets.FirstOrDefault(w => ReferenceEquals(w, item.Presenter));
                        if (widget != null)
                        {
                            item.Owner = null;
                            _container.Widgets.Remove(widget);
                        }

                        item.RemovePresenterChangedHandler(OnItemPresenterChanged);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (CommandItemBase<TBar, TItemBase> item in _innerItems)
                    {
                        item.Owner = null;
                        item.RemovePresenterChangedHandler(OnItemPresenterChanged);
                    }

                    _container.Widgets.Clear();
                    break;
            }
        }

        private void OnItemPresenterChanged(CommandItemBase<TBar, TItemBase> item)
        {
            for (int i = 0; i < _container.Widgets.Count; i++)
            {
                if (ReferenceEquals(_container.Widgets[i], item.Presenter))
                    return; // schon aktuell

                if (_container.Widgets[i] == item.Presenter) // alten Presenter ersetzen
                {
                    _container.Widgets[i] = item.Presenter;
                    return;
                }
            }
        }

    }
}
