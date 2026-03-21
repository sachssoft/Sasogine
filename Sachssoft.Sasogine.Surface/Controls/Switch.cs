using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.IO.Pipes;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class Switch : Widget, ICheckable
    {
        private StyleProperty<bool> _isChecked = new StyleProperty<bool>(false, false);
        private StyleProperty<TimeSpan> _animationDuration = new StyleProperty<TimeSpan>(TimeSpan.FromMilliseconds(200), false);

        private TimeSpan _animationElapsed = TimeSpan.Zero;
        private float _animationStart = 0f;
        private float _animationEnd = 0f;
        private bool _isAnimating = false;

        private Point _fixedSize = new Point();
        private readonly Image _knob = new();
        private readonly StackPanelLayout _layout = new StackPanelLayout(Visuals.Orientation.Horizontal);

        #region Events 

        public event EventHandler? IsCheckedChanged;

        #endregion

        public Switch()
        {
            _knob = new StateIndicator
            {
                HorizontalAlignment = Visuals.HorizontalAlignment.Left,
                VerticalAlignment = Visuals.VerticalAlignment.Top,
            };

            LayoutContainer  = _layout;
            Children.Add(_knob);
        }

        #region Style Properties

        public StyleProperty<bool> IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetAndNotify(ref _isChecked, value))
                {
                    InvalidateArrange();
                }
            }
        }

        public StyleProperty<TimeSpan> AnimationDuration
        {
            get => _animationDuration;
            set => SetAndNotify(ref _animationDuration, value);
        }

        #endregion

        #region Direct Properties

        public Image Knob => _knob;

        #endregion

        #region Layout

        protected override void InternalArrange()
        {
            base.InternalArrange();

            // Nur initial setzen, wenn Knob noch nie positioniert wurde
            if (_knob.Left == 0 && !IsChecked.Value)
                _knob.Left = 0;
            else if (_knob.Left == 0 && IsChecked.Value)
                _knob.Left = Bounds.Width - _knob.Bounds.Width;
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            var knobStyle = style?.FindStyle(typeof(Image), "knob");
            _knob.ApplyFromStyle(knobStyle);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(IsChecked):
                        target.IsChecked = target.IsChecked.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(AnimationDuration):
                        target.AnimationDuration = target.AnimationDuration.Override(TimeSpan.FromMilliseconds(value.ConvertTo<float>()));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not Switch source)
                return;

            IsChecked = source.IsChecked;
            AnimationDuration = source.AnimationDuration;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new Switch();
        }

        #endregion

        protected internal override void OnTouchUp()
        {
            base.OnTouchUp();

            // Toggle
            IsChecked = !IsChecked.Value;

            // Animation sofort neu starten
            ResetAnimation();
        }

        public override void InternalRender(RenderContext context, GameTime time)
        {
            Update(time);
            base.InternalRender(context, time);
        }

        #region Helpers

        private void Update(GameTime gameTime)
        {
            // Immer prüfen, ob Ziel sich geändert hat oder Animation zurückgesetzt werden soll
            float target = GetAnimationTarget();

            if (_isAnimating || Math.Abs(_animationEnd - target) > 0.001f)
            {
                AnimateKnob(gameTime);
            }
        }

        private float GetAnimationTarget()
        {
            return IsChecked.Value ? Bounds.Width - _knob.Bounds.Width : 0f;
        }

        private void ResetAnimation()
        {
            // Animation sofort von aktueller Position zum Ziel starten
            _animationStart = _knob.Left;
            _animationEnd = GetAnimationTarget();
            _animationElapsed = TimeSpan.Zero;
            _isAnimating = true;
        }

        private void AnimateKnob(GameTime gameTime)
        {
            _animationElapsed += gameTime.ElapsedGameTime;
            float t = (float)(_animationElapsed.TotalMilliseconds / _animationDuration.Value.TotalMilliseconds);

            if (t >= 1f)
            {
                t = 1f;
                _isAnimating = false;
            }

            t = 0.5f - 0.5f * MathF.Cos(t * MathF.PI);

            _knob.Left = (int)MathHelper.Lerp(_animationStart, _animationEnd, t);
        }

        #endregion

        #region ICheckable

        bool ICheckable.IsChecked
        {
            get => IsChecked.Value;
            set => IsChecked = value;
        }

        #endregion
    }
}
