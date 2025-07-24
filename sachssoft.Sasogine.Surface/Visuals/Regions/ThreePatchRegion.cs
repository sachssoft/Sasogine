using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a simplified "three-patch" texture region that stretches in one direction (horizontal or vertical).
    /// </summary>
    /// <remarks>
    /// This class splits a texture region into three parts:
    /// <list type="bullet">
    /// <item><description><b>First segment:</b> fixed size (left or top)</description></item>
    /// <item><description><b>Middle segment:</b> stretchable or shrinkable</description></item>
    /// <item><description><b>Last segment:</b> fixed size (right or bottom)</description></item>
    /// </list>
    /// It is a simpler alternative to the "nine-patch" technique,
    /// useful for UI elements like buttons, panels, or progress bars.
    /// </remarks>
    public class ThreePatchRegion : IImage
    {
        private readonly Orientation _orientation;
        private readonly int _fixedA;
        private readonly int _fixedB;

        private readonly TextureRegion _first;
        private readonly TextureRegion _middle;
        private readonly TextureRegion _last;

        /// <summary>
        /// Gets the total size of the three-patch region combining fixed and stretchable parts.
        /// </summary>
        public Point Size
        {
            get
            {
                if (_orientation == Orientation.Horizontal)
                {
                    int width = _fixedA + _middle.Bounds.Width + _fixedB;
                    int height = _first.Bounds.Height;
                    return new Point(width, height);
                }
                else
                {
                    int width = _first.Bounds.Width;
                    int height = _fixedA + _middle.Bounds.Height + _fixedB;
                    return new Point(width, height);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreePatchRegion"/> class
        /// by slicing a single texture region into three segments using fixed sizes.
        /// </summary>
        /// <param name="texture">The source texture.</param>
        /// <param name="bounds">The rectangle bounds of the whole region within the texture.</param>
        /// <param name="fixedA">The fixed size of the first segment (left or top).</param>
        /// <param name="fixedB">The fixed size of the last segment (right or bottom).</param>
        /// <param name="orientation">The orientation to stretch: horizontal or vertical.</param>
        public ThreePatchRegion(Texture2D texture, Rectangle bounds, int fixedA, int fixedB, Orientation orientation)
        {
            _orientation = orientation;
            _fixedA = fixedA;
            _fixedB = fixedB;

            if (orientation == Orientation.Horizontal)
            {
                int middleWidth = Math.Max(0, bounds.Width - fixedA - fixedB);

                int xLeft = bounds.X;
                int xMiddle = bounds.X + fixedA;
                int xRight = bounds.X + fixedA + middleWidth;

                _first = new TextureRegion(texture, new Rectangle(xLeft, bounds.Y, fixedA, bounds.Height));
                _middle = new TextureRegion(texture, new Rectangle(xMiddle, bounds.Y, middleWidth, bounds.Height));
                _last = new TextureRegion(texture, new Rectangle(xRight, bounds.Y, fixedB, bounds.Height));
            }
            else
            {
                int middleHeight = Math.Max(0, bounds.Height - fixedA - fixedB);

                int yTop = bounds.Y;
                int yMiddle = bounds.Y + fixedA;
                int yBottom = bounds.Y + fixedA + middleHeight;

                _first = new TextureRegion(texture, new Rectangle(bounds.X, yTop, bounds.Width, fixedA));
                _middle = new TextureRegion(texture, new Rectangle(bounds.X, yMiddle, bounds.Width, middleHeight));
                _last = new TextureRegion(texture, new Rectangle(bounds.X, yBottom, bounds.Width, fixedB));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreePatchRegion"/> class
        /// from explicit texture regions for each segment.
        /// </summary>
        /// <param name="first">The fixed first segment (left or top).</param>
        /// <param name="middle">The stretchable middle segment.</param>
        /// <param name="last">The fixed last segment (right or bottom).</param>
        /// <param name="orientation">The stretch orientation (horizontal or vertical).</param>
        /// <exception cref="ArgumentNullException">Thrown if any segment is null.</exception>
        public ThreePatchRegion(TextureRegion first, TextureRegion middle, TextureRegion last, Orientation orientation)
        {
            _first = first ?? throw new ArgumentNullException(nameof(first));
            _middle = middle ?? throw new ArgumentNullException(nameof(middle));
            _last = last ?? throw new ArgumentNullException(nameof(last));
            _orientation = orientation;

            if (orientation == Orientation.Horizontal)
            {
                _fixedA = first.Bounds.Width;
                _fixedB = last.Bounds.Width;
            }
            else
            {
                _fixedA = first.Bounds.Height;
                _fixedB = last.Bounds.Height;
            }
        }

        /// <summary>
        /// Draws the three-patch region within the specified destination rectangle,
        /// stretching the middle segment to fill the remaining space.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="dest">The destination rectangle to draw into.</param>
        /// <param name="color">The tint color to apply.</param>
        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            if (_orientation == Orientation.Horizontal)
            {
                int left = Math.Min(_fixedA, dest.Width);
                int right = Math.Min(_fixedB, dest.Width - left);
                int middleWidth = Math.Max(0, dest.Width - left - right);

                int xLeft = dest.X;
                int xMiddle = dest.X + left;
                int xRight = dest.X + left + middleWidth;

                _first.Draw(context, new Rectangle(xLeft, dest.Y, left, dest.Height), color);
                if (middleWidth > 0)
                    _middle.Draw(context, new Rectangle(xMiddle, dest.Y, middleWidth, dest.Height), color);
                _last.Draw(context, new Rectangle(xRight, dest.Y, right, dest.Height), color);
            }
            else
            {
                int top = Math.Min(_fixedA, dest.Height);
                int bottom = Math.Min(_fixedB, dest.Height - top);
                int middleHeight = Math.Max(0, dest.Height - top - bottom);

                int yTop = dest.Y;
                int yMiddle = dest.Y + top;
                int yBottom = dest.Y + top + middleHeight;

                _first.Draw(context, new Rectangle(dest.X, yTop, dest.Width, top), color);
                if (middleHeight > 0)
                    _middle.Draw(context, new Rectangle(dest.X, yMiddle, dest.Width, middleHeight), color);
                _last.Draw(context, new Rectangle(dest.X, yBottom, dest.Width, bottom), color);
            }
        }
    }
}
