using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class RepeatButton : ButtonBase
    {
        private StyleProperty<TimeSpan> _interval = new StyleProperty<TimeSpan>(TimeSpan.FromMilliseconds(100), isUserSet: false);
        private StyleProperty<TimeSpan> _delay = new StyleProperty<TimeSpan>(TimeSpan.FromMilliseconds(400), isUserSet: false);

        private double _nextClickTime;
        private bool _isHolding;
        private double _elapsedTime;
        private bool _delayElapsed;

        public RepeatButton()
        {
        }

        #region Style Properties

        public StyleProperty<TimeSpan> Interval
        {
            get => _interval;
            set => SetAndNotify(ref _interval, value);
        }

        public StyleProperty<TimeSpan> Delay
        {
            get => _delay;
            set => SetAndNotify(ref _delay, value);
        }

        #endregion

        protected override void InternalOnTouchDown()
        {
            if (!IsEnabled) return;

            _isHolding = true;
            _elapsedTime = 0;
            _delayElapsed = false;

            // Sofortiger erster Klick
            OnClick(EventArgs.Empty);
        }

        protected override void InternalOnTouchUp()
        {
            _isHolding = false;
            _elapsedTime = 0;
            _delayElapsed = false;
            IsPressed = false;
        }

        protected internal override void OnTouchMove()
        {
            base.OnTouchMove();

            if (!_isHolding)
                return;

            // Optional: Abbrechen, wenn Finger/Bereich verlassen wird
        }

        public override void InternalRender(RenderContext context, GameTime time)
        {
            base.InternalRender(context, time);

            Update(time);
        }

        private void Update(GameTime gameTime)
        {
            if (!_isHolding || !IsEnabled) return;

            _elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!_delayElapsed)
            {
                if (_elapsedTime >= Delay.Value.TotalMilliseconds)
                {
                    _delayElapsed = true;
                    _elapsedTime -= Delay.Value.TotalMilliseconds;
                    OnClick(EventArgs.Empty);
                }
            }
            else
            {
                while (_elapsedTime >= Interval.Value.TotalMilliseconds)
                {
                    _elapsedTime -= Interval.Value.TotalMilliseconds;
                    OnClick(EventArgs.Empty);
                }
            }
        }

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            // Style für das Content-Label anwenden
            var contentLabelStyle = style?.FindStyle(typeof(Label), "content");
            if (ContentPresenter is Label label)
                label.ApplyFromStyle(contentLabelStyle);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Background):
                        target.Background = target.Background.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(HoveredBackground):
                        target.HoveredBackground = target.HoveredBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(PressedBackground):
                        target.PressedBackground = target.PressedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(Interval):
                        target.Interval = target.Interval.Override(TimeSpan.FromMilliseconds(value.ConvertTo<float>()));
                        break;
                    case nameof(Delay):
                        target.Delay = target.Delay.Override(TimeSpan.FromMilliseconds(value.ConvertTo<float>()));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not RepeatButton source)
                return;

            Background = source.Background;
            HoveredBackground = source.HoveredBackground;
            PressedBackground = source.PressedBackground;
            Interval = source.Interval;
            Delay = source.Delay;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new RepeatButton();
        }

        #endregion
    }
}
