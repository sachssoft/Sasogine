using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using System.Reflection.Emit;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class NumberSpinner : RangeBase
    {
        private StyleProperty<float> _smallChange = new StyleProperty<float>(1f, isUserSet: false);
        private StyleProperty<float> _largeChange = new StyleProperty<float>(10f, isUserSet: false);
        private StyleProperty<NumberSpinnerType> _type = new StyleProperty<NumberSpinnerType>(NumberSpinnerType.Classic, isUserSet: false);

        private bool _changedByUser;
        private readonly GridLayout _layout = new GridLayout();
        private readonly RepeatButton _decrementButton = new RepeatButton();
        private readonly RepeatButton _incrementButton = new RepeatButton();
        private readonly Image _decrementButtonIndicator = new Image();
        private readonly Image _incrementButtonIndicator = new Image();
        private readonly TextBox _valueBox = new TextBox();

        public NumberSpinner()
        {
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

            Grid.SetColumn(_decrementButton, 0);
            Grid.SetColumn(_valueBox, 1);
            Grid.SetColumn(_incrementButton, 2);

            LayoutContainer  = _layout;

            Children.Add(_decrementButton);
            Children.Add(_incrementButton);
            Children.Add(_valueBox);

            _decrementButton.Content = _decrementButtonIndicator;
            _incrementButton.Content = _incrementButtonIndicator;

            _decrementButton.Click += DecrementButton_Click;
            _incrementButton.Click += IncrementButton_Click;

            UpdateText();
        }

        #region Style Properties

        public StyleProperty<float> SmallChange
        {
            get => _smallChange;
            set => SetAndNotify(ref _smallChange, value);
        }

        public StyleProperty<float> LargeChange
        {
            get => _largeChange;
            set => SetAndNotify(ref _largeChange, value);
        }

        public StyleProperty<NumberSpinnerType> Type
        {
            get => _type;
            set => SetAndNotify(ref _type, value);
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            var decrementStyle = style?.FindStyle(typeof(RepeatButton), "decrement");
            if (decrementStyle != null)
            {
                _decrementButton.ApplyFromStyle(decrementStyle);
                _decrementButtonIndicator.ApplyFromStyle(decrementStyle.FindStyle(typeof(Image), "indicator"));
            }

            var incrementStyle = style?.FindStyle(typeof(RepeatButton), "increment");
            if (incrementStyle != null)
            {
                _incrementButton.ApplyFromStyle(incrementStyle);
                _incrementButtonIndicator.ApplyFromStyle(incrementStyle.FindStyle(typeof(Image), "indicator"));
            }

            var valueBoxStyle = style?.FindStyle(typeof(TextBox), "value");
            _valueBox.ApplyFromStyle(valueBoxStyle);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(SmallChange):
                        target.SmallChange = target.SmallChange.Override(value.ConvertTo<float>());
                        break;

                    case nameof(LargeChange):
                        target.LargeChange = target.LargeChange.Override(value.ConvertTo<float>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not NumberSpinner source)
                return;

            SmallChange = source.SmallChange;
            LargeChange = source.LargeChange;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new NumberSpinner();
        }

        #endregion

        private void IncrementButton_Click(object? sender, System.EventArgs e)
        {
            Value += SmallChange.Value;
        }

        private void DecrementButton_Click(object? sender, System.EventArgs e)
        {
            Value -= SmallChange.Value;
        }

        internal override void OnValueChanged(ValueChangedEventArgs<float> e)
        {
            base.OnValueChanged(e);

            if (!_changedByUser)
                UpdateText();
        }

        public override void OnKeyUp(Keys k)
        {
            base.OnKeyUp(k);

            if (k == Keys.Enter)
            {
                EnsureReturnValue();
                RequestKeyboardFocus();
            }
        }

        public override void OnKeyboardFocusLost()
        {
            base.OnKeyboardFocusLost();
            EnsureReturnValue();
        }

        private void EnsureReturnValue()
        {

            if (float.TryParse(_valueBox.Text, out var result))
            {
                _changedByUser = true;
                Value = result;
                _changedByUser = false;
            }
            else
            {
                UpdateText();
            }
        }

        private void UpdateText()
        {
            _valueBox.Text = ValueDisplay;
        }
    }
}
