using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public abstract class WrapPanel : Container
    {
        private readonly WrapPanelLayout _layout;

        public abstract Orientation Orientation { get; }

        public int Spacing
        {
            get => _layout.Spacing;
            set => _layout.Spacing = value;
        }

        protected WrapPanel()
        {
            _layout = new WrapPanelLayout(Orientation);
            LayoutContainer  = _layout;
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not WrapPanel source)
                return;

            Spacing = source.Spacing;
        }
    }
}
