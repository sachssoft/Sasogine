using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Basic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public abstract class LayoutBase : FrameBase
    {
        public LayoutBase()
        {
            Layer = FrameLayer.Background;
        }

        public FrameCollection Children => ChildFrames;

        protected void RenderChildren(GameTime gameTime, FrameContext context)
        {
            foreach (var child in Children)
            {
                if (child.IsVisible)
                    child.Render(gameTime, context);
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children)
            {
                child.Measure(availableSize);
                size.X = float.Max(size.X, child.DesiredSize.X + child.Margin.Horizontal);
                size.Y = float.Max(size.Y, child.DesiredSize.Y + child.Margin.Vertical);
            }
            size.X += Padding.Horizontal;
            size.Y += Padding.Vertical;
            return size;
        }

        protected override Bounds ArrangeOverride(Bounds finalBounds)
        {
            foreach (var child in Children)
            {
                var childBounds = new Bounds(
                    finalBounds.X + child.Margin.Left,
                    finalBounds.Y + child.Margin.Top,
                    child.DesiredSize.X,
                    child.DesiredSize.Y
                );
                child.Arrange(childBounds);
            }
            return finalBounds;
        }

        protected internal override void Render(GameTime gameTime, FrameContext context)
        {
            RenderChildren(gameTime, context);
        }
    }
}
