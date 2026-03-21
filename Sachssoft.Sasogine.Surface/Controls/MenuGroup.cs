using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class MenuGroup : MenuItemBase, IMenuNode
    {
        private StyleProperty<ITextureRegion?> _icon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<object?> _header = new StyleProperty<object?>(null, isUserSet: false);

        private readonly MenuItemView _view = new MenuItemView();
        private bool _isLoaded;

        #region Events

        public event EventHandler? IsCheckedChanged;

        #endregion

        public MenuGroup()
        {
            _view.Items.CollectionChanged += Items_CollectionChanged;
        }

        #region Style Properties

        public StyleProperty<ITextureRegion?> Icon
        {
            get => _icon;
            set
            {
                if (SetAndNotify(ref _icon, value))
                {
                    UpdateView();
                }
            }
        }

        public StyleProperty<object?> Header
        {
            get => _header;
            set
            {
                if (SetAndNotify(ref _header, value))
                {
                    UpdateView();
                }
            }
        }

        #endregion

        #region Direct Properties

        public ObservableCollection<MenuItemBase> Items => _view.Items;

        internal protected override Widget Presenter
        {
            get => _view ?? BuildPresenter();
        }

        #endregion

        #region Helpers

        private MenuItemView BuildPresenter()
        {
            UnloadPresenter();
            UpdateView();

            return _view;
        }

        private void UpdateView()
        {
            if (_view != null)
            {
                _view.Icon = new StyleProperty<ITextureRegion?>(_icon.Value);
                _view.Header = new StyleProperty<object?>(_header.Value);
                _view.Margin = new StyleProperty<Visuals.Thickness>(new Visuals.Thickness(5));

                if (Parent != null)
                    _view.Mode = new StyleProperty<MenuItemViewMode>(MenuItemViewMode.Underlying);

                if (Owner is Menu menu)
                {
                    _view.IconWidth = menu.IconWidth.Value + menu.IconMargin.Value.Left + menu.IconMargin.Value.Right;
                    _view.IconHeight = menu.IconHeight.Value + menu.IconMargin.Value.Top + menu.IconMargin.Value.Bottom;
                    _view.IconMargin = menu.IconMargin.Value;
                }
            }
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (MenuItemBase item in e.NewItems!)
                    {
                        // Parent auf diese MenuGroup setzen
                        item.Parent = this;

                        // Owner von MenuGroup vererben (optional)
                        if (this.Owner != null)
                            item.Owner = this.Owner;

                        // Falls Presenter schon existiert, ggf. aktualisieren
                        item.AddPresenterChangedHandler(OnChildPresenterChanged);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (MenuItemBase item in e.OldItems!)
                    {
                        item.Parent = null;
                        item.Owner = null;
                        item.RemovePresenterChangedHandler(OnChildPresenterChanged);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (MenuItemBase item in Items)
                    {
                        item.Parent = null;
                        item.Owner = null;
                        item.RemovePresenterChangedHandler(OnChildPresenterChanged);
                    }
                    break;
            }
        }
        private void OnChildPresenterChanged(CommandItemBase<Menu, MenuItemBase> item)
        {
            // Hier kannst du z.B. ein Submenu-Widget neu rendern
            InvalidatePresenter();
        }

        private void UnloadPresenter()
        {
        }

        protected override void OnOwnerPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnOwnerPropertyChanged(e);
            UpdateView();
        }

        #endregion

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MenuGroup source)
                return;

            Icon = source.Icon;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuGroup();
        }

        #endregion
    }
}
