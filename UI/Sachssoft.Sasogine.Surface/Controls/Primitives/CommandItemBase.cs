using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{

    public abstract class CommandItemBase<TBar, TItemBase> : ElementBase
        where TBar : CommandBarBase<TBar, TItemBase>
        where TItemBase : CommandItemBase<TBar, TItemBase>
    {
        private Action<CommandItemBase<TBar, TItemBase>>? _presenterChanged;
        private ElementBase? _owner;

        public CommandItemBase()
        {
        }

        #region Direct Properties

        internal protected abstract Widget Presenter { get; }

        public ElementBase? Owner
        {
            get => _owner;
            internal set
            {
                if (SetAndNotify(ref _owner, value, out var oldValue))
                {
                    if (oldValue != null)
                    {
                        oldValue.PropertyChanged -= Owner_PropertyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += Owner_PropertyChanged;
                    }
                }
            }
        }

        #endregion

        protected override void OnIsVisibleChanged(EventArgs e)
        {
            base.OnIsVisibleChanged(e);

            var presenter = Presenter;
            if (presenter != null)
                presenter.IsVisible = IsVisible.Value;
        }

        internal void AddPresenterChangedHandler(Action<CommandItemBase<TBar, TItemBase>> handler)
        {
            _presenterChanged = (Action<CommandItemBase<TBar, TItemBase>>)Delegate.Combine(_presenterChanged, handler);
        }

        internal void RemovePresenterChangedHandler(Action<CommandItemBase<TBar, TItemBase>> handler)
        {
            _presenterChanged = (Action<CommandItemBase<TBar, TItemBase>>?)Delegate.Remove(_presenterChanged, handler);
        }

        private void Owner_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnOwnerPropertyChanged(e);
        }

        protected virtual void OnOwnerPropertyChanged(PropertyChangedEventArgs e)
        {
        }

        protected virtual void InvalidatePresenter()
        {
            _presenterChanged?.Invoke(this);
        }

        public virtual void UpdateLayout(Rectangle containerBounds)
        {
        }
    }
}
