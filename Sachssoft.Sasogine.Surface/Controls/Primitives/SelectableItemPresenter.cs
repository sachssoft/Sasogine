using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public class SelectableItemPresenter : ContentControl, ISelectable
    {
        private StyleProperty<bool> _isSelectable = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<bool> _isSelected = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<IBrush?> _selectedBackground = new StyleProperty<IBrush?>(DefaultStyle.HighlightBrush, isUserSet: false);

        private bool _isClicked;

        public event EventHandler? Click;
        public event EventHandler<ValueChangedEventArgs<bool>>? IsSelectedChanged;
        public event EventHandler<ValueChangingEventArgs<bool>>? IsSelectedChangingByUser;
        public event EventHandler<ValueChangedEventArgs<bool>>? IsSelectedChangedByUser;

        public SelectableItemPresenter()
        {
            //HoveredBackground.Override(DefaultStyle.HighlightBrush);
        }

        #region Style Properties

        public StyleProperty<bool> IsSelectable
        {
            get => _isSelectable;
            set
            {
                if (SetAndNotify(ref _isSelectable, value))
                {
                }
            }
        }

        public StyleProperty<bool> IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetAndNotify(ref _isSelected, value, out var oldValue))
                {
                    OnIsSelectedChanged(new ValueChangedEventArgs<bool>(oldValue, value));
                }
            }
        }

        public StyleProperty<IBrush?> SelectedBackground
        {
            get => _selectedBackground;
            set
            {
                if (SetAndNotify(ref _selectedBackground, value))
                {
                }
            }
        }

        #endregion

        #region Rendering

        public override IBrush? GetCurrentBackground()
        {
            var result = base.GetCurrentBackground();

            if (IsEffectiveEnabled)
            {
                if (IsSelectable.Value && IsSelected.Value && SelectedBackground.Value != null)
                {
                    result = SelectedBackground.Value;
                }
                else if (UseOverBackground && HoveredBackground.Value != null)
                {
                    result = HoveredBackground.Value;
                }
            }

            return result;
        }

        #endregion

        #region Interaction

        protected internal override void OnTouchDown()
        {
            base.OnTouchDown();
            if (!IsEffectiveEnabled) return;
            _isClicked = true;
        }

        protected internal override void OnTouchUp()
        {
            base.OnTouchUp();
            if (!IsEffectiveEnabled) return;

            if (_isClicked && IsEffectiveEnabled)
            {
                _isClicked = false;
                OnClick(EventArgs.Empty);
                SetSelectionByUser(!IsSelected);
            }
        }

        internal virtual void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }

        protected void SetSelectionByUser(bool value)
        {
            if (!IsSelectable.Value)
                return;

            if (value != IsSelected.Value)
            {
                var e = new ValueChangingEventArgs<bool>(_isSelected.Value, value);
                OnIsSelectedChangingByUser(e);
                if (e.Cancel) return;
                OnIsSelectedChangedByUser(new ValueChangedEventArgs<bool>(e.OldValue, e.NewValue));
            }

            IsSelected = value;
        }

        protected virtual void OnIsSelectedChanged(ValueChangedEventArgs<bool> e)
        {
            IsSelectedChanged?.Invoke(this, e);
        }

        protected virtual void OnIsSelectedChangingByUser(ValueChangingEventArgs<bool> e)
        {
            IsSelectedChangingByUser?.Invoke(this, e);
        }

        protected virtual void OnIsSelectedChangedByUser(ValueChangedEventArgs<bool> e)
        {
            IsSelectedChangedByUser?.Invoke(this, e);
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
                    case nameof(IsSelectable):
                        target.IsSelectable = target.IsSelectable.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(IsSelected):
                        target.IsSelected = target.IsSelected.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(SelectedBackground):
                        target.SelectedBackground = target.SelectedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not SelectableItemPresenter source)
                return;

            IsSelectable = source.IsSelectable;
            IsSelected = source.IsSelected;
            SelectedBackground = source.SelectedBackground;
        }

        #endregion

        #region ISelectable

        bool ISelectable.IsSelectable
        {
            get => IsSelectable.Value;
            set => IsSelectable = new StyleProperty<bool>(value);
        }

        bool ISelectable.IsSelected
        {
            get => IsSelected.Value;
            set => IsSelected = new StyleProperty<bool>(value);
        }

        #endregion
    }
}