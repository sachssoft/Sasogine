using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Basic
{
    public readonly struct LayoutBounds
    {
        private readonly Bounds _container;
        private readonly Bounds _content;
        private readonly Insets _padding;

        public LayoutBounds(Bounds container, Insets padding)
        {
            _container = container;
            _content = new Bounds(
                container.X + padding.Left,
                container.Y + padding.Top,
                container.Width - padding.Left - padding.Right,
                container.Height - padding.Top - padding.Bottom
            );
            _padding = padding;
        }

        public Bounds Container => _container;

        public Bounds Content => _content;

        public Insets Padding => _padding;
    }
}
