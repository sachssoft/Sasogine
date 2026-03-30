using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public readonly struct Insets
    {
        private readonly float _left;
        private readonly float _top;
        private readonly float _right;
        private readonly float _bottom;


        public static readonly Insets None = new Insets(0.0f);

        public Insets(float uniform)
        {
            _left = uniform;
            _top = uniform;
            _right = uniform;
            _bottom = uniform;
        }

        public Insets(float horizontal, float vertical)
        {
            _left = horizontal;
            _right = horizontal;
            _top = vertical;
            _bottom = vertical;
        }

        public Insets(float left, float top, float right, float bottom)
        {
            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
        }

        public float Left => _left;
        public float Top => _top;
        public float Right => _right;
        public float Bottom => _bottom;

        public float Horizontal => _left + _right;
        public float Vertical => _top + _bottom;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Apply(in Bounds bounds)
            => new Bounds(
                bounds.Left + _left,
                bounds.Top + _top,
                bounds.Width - Horizontal,
                bounds.Height - Vertical
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bounds Expand(in Bounds bounds)
            => new Bounds(
                bounds.Left - _left,
                bounds.Top - _top,
                bounds.Width + Horizontal,
                bounds.Height + Vertical
            );

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}, {1}, {2}, {3}",
                Left,
                Top,
                Right,
                Bottom
            );
        }
    }
}