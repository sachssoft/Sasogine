using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Styles;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class RangeBase : Widget
    {
        private StyleProperty<float> _value = new StyleProperty<float>(0f, isUserSet: false);
        private StyleProperty<string?> _valueFormat = new StyleProperty<string?>("0", isUserSet: false);
        private StyleProperty<CultureInfo?> _valueFormatCulture = new StyleProperty<CultureInfo?>(null, isUserSet: false);
        private StyleProperty<float> _minimum = new StyleProperty<float>(0f, isUserSet: false);
        private StyleProperty<float> _maximum = new StyleProperty<float>(100f, isUserSet: false);

        #region Events

        /// <summary>
        /// Fires when the value had been changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<float>>? ValueChanged;

        /// <summary>
        /// Fires only when the value had been changed by user(doesnt fire if it had been assigned through code)
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<float>>? ValueChangedByUser;

        #endregion

        #region Style Properties

        public StyleProperty<float> Value
        {
            get => _value;
            set
            {
                var _oldValue = _value;
                if (SetAndNotify(ref _value, value))
                {
                    ClampValue(); // sicherstellen, dass Value innerhalb Min/Max bleibt
                    OnValueChanged(new ValueChangedEventArgs<float>(_oldValue, _value));
                }
            }
        }

        public StyleProperty<string?> ValueFormat
        {
            get => _valueFormat;
            set => SetAndNotify(ref _valueFormat, value);
        }

        public StyleProperty<CultureInfo?> ValueFormatCulture
        {
            get => _valueFormatCulture;
            set => SetAndNotify(ref _valueFormatCulture, value);
        }

        public StyleProperty<float> Minimum
        {
            get => _minimum;
            set
            {
                if (SetAndNotify(ref _minimum, value))
                {
                    ClampValue();
                }
            }
        }

        public StyleProperty<float> Maximum
        {
            get => _maximum;
            set
            {
                if (SetAndNotify(ref _maximum, value))
                {
                    ClampValue();
                }
            }
        }

        #endregion

        #region Direct Properties

        public string? ValueDisplay => Value.Value.ToString(
            ValueFormat.Value, 
            ValueFormatCulture.Value ?? UIEnvironment.Culture
        );

        #endregion

        internal virtual void OnValueChanged(ValueChangedEventArgs<float> e)
        {
            ValueChanged?.Invoke(this, e);
        }

        internal virtual void OnValueChangedByUser(ValueChangedEventArgs<float> e)
        {
            ValueChangedByUser?.Invoke(this, e);
        }

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Value):
                        target.Value = target.Value.Override(value.ConvertTo<float>());
                        break;

                    case nameof(ValueFormat):
                        target.ValueFormat = target.ValueFormat.Override(!string.IsNullOrEmpty(value.RawValue) ? value.RawValue : "0");
                        break;

                    case nameof(ValueFormatCulture):
                        target.ValueFormatCulture = target.ValueFormatCulture.Override(value.ConvertTo<CultureInfo>());
                        break;

                    case nameof(Minimum):
                        target.Minimum = target.Minimum.Override(value.ConvertTo<float>());
                        break;

                    case nameof(Maximum):
                        target.Maximum = target.Maximum.Override(value.ConvertTo<float>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not RangeBase source)
                return;

            Value = source.Value;
            ValueFormat = source.ValueFormat;
            ValueFormatCulture = source.ValueFormatCulture;
            Minimum = source.Minimum;
            Maximum = source.Maximum;
        }

        #endregion

        #region Helpers

        // Hilfsmethode zum Clampen
        private void ClampValue()
        {
            if (_value < _minimum)
            {
                _value = _minimum;
            }

            if (_value > _maximum)
            {
                _value = _maximum;
            }
        }

        #endregion
    }
}
