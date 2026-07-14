using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Presentation.Layouts
{
    public readonly struct LayoutBounds
    {
        private readonly Bounds _container;
        private readonly Bounds _content;
        private readonly Bounds _extent;
        private readonly Insets _padding;
        private readonly Insets _margin;

        public LayoutBounds(Bounds container)
            : this(container, Insets.None, Insets.None) { }

        public LayoutBounds(Bounds container, Insets margin)
            : this(container, Insets.None, margin) { }

        public LayoutBounds(Bounds container, Insets padding, Insets margin)
        {
            _container = container;
            _content = new Bounds(
                container.X + padding.Left,
                container.Y + padding.Top,
                container.Width - padding.Left - padding.Right,
                container.Height - padding.Top - padding.Bottom
            );
            _extent = new Bounds(
                container.X - margin.Left,
                container.Y - margin.Top,
                container.Width + margin.Left + margin.Right,
                container.Height + margin.Top + margin.Bottom
            );
            _padding = padding;
            _margin = margin;
        }

        public Bounds Container => _container;

        public Bounds Content => _content;

        public Bounds Extent => _extent;

        public Insets Padding => _padding;

        public Insets Margin => _margin;
    }
}
