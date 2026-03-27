using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class CheckableButtonBase : ButtonBase, ICheckable
    {
        private StyleProperty<ITextureRegion?> _uncheckedImage = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<ITextureRegion?> _checkedImage = new StyleProperty<ITextureRegion?>(null, isUserSet: false);

        private readonly StateIndicator _stateIndicator;

        public event EventHandler? IsCheckedChanged
        {
            add => PressedChanged += value;
            remove => PressedChanged -= value;
        }

        protected CheckableButtonBase()
        {
            _stateIndicator = new StateIndicator
            {
                HorizontalAlignment = Visuals.HorizontalAlignment.Left,
                VerticalAlignment = Visuals.VerticalAlignment.Center,
            };

            //UpdateChildren();
        }

        #region Style Properties

        public StyleProperty<bool> IsChecked
        {
            get => IsPressed;
            set
            {
                if (IsPressed.Value != value)
                {
                    OnPropertyChanging(nameof(IsChecked));
                    IsPressed = value;
                    UpdateImage();
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public StyleProperty<ITextureRegion?> UncheckedImage
        {
            get => _uncheckedImage;
            set
            {
                if (SetAndNotify(ref _uncheckedImage, value))
                {
                    UpdateImage();
                }
            }
        }

        public StyleProperty<ITextureRegion?> CheckedImage
        {
            get => _checkedImage;
            set
            {
                if (SetAndNotify(ref _checkedImage, value))
                {
                    UpdateImage();
                }
            }
        }

        #endregion

        #region Direct Properties

        protected StateIndicator StateIndicator => _stateIndicator;

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(UncheckedImage):
                        target.UncheckedImage = target.UncheckedImage.Override(sheet.FindRegion(value.RawValue));
                        break;

                    case nameof(CheckedImage):
                        target.CheckedImage = target.CheckedImage.Override(sheet.FindRegion(value.RawValue));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not CheckableButtonBase source)
                return;

            UncheckedImage = source.CheckedImage;
            CheckedImage = source.CheckedImage;
        }

        #endregion

        protected override void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(IsPressed):
                    base.OnPropertyChanging(nameof(IsChecked));
                    break;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(IsPressed):
                    base.OnPropertyChanged(nameof(IsChecked));
                    break;
            }
        }

        protected override void InternalOnTouchUp()
        {
        }

        protected override void InternalOnTouchDown()
        {
            SetValueByUser(!IsPressed);
        }

        public override void OnKeyDown(Keys k)
        {
            base.OnKeyDown(k);

            if (!IsEnabled)
            {
                return;
            }

            if (k == Keys.Space)
            {
                SetValueByUser(!IsPressed);
            }
        }

        protected override void OnPressedChanged(EventArgs e)
        {
            base.OnPressedChanged(e);

            _stateIndicator.IsPressed = IsPressed;
        }

        private void UpdateImage()
        {
            _stateIndicator.Visual = IsPressed.Value ?
                _checkedImage :
                _uncheckedImage;
        }

        #region ICheckable

        bool ICheckable.IsChecked
        {
            get => IsChecked.Value;
            set => IsChecked = value;
        }

        #endregion
    }
}