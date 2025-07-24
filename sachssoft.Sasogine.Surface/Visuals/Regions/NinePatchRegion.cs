using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a nine-patch texture region, which allows scaling a texture while preserving its borders.
    /// </summary>
    /// <remarks>
    /// A nine-patch texture is divided into 9 regions:
    /// - 4 corners remain fixed.
    /// - 4 edges stretch only in one direction.
    /// - The center stretches both horizontally and vertically.
    /// </remarks>
    public class NinePatchRegion : TextureRegion
    {
        private readonly Thickness _info;

        // Sub-regions of the nine-patch
        private readonly TextureRegion? _topLeft;
        private readonly TextureRegion? _topCenter;
        private readonly TextureRegion? _topRight;
        private readonly TextureRegion? _centerLeft;
        private readonly TextureRegion? _center;
        private readonly TextureRegion? _centerRight;
        private readonly TextureRegion? _bottomLeft;
        private readonly TextureRegion? _bottomCenter;
        private readonly TextureRegion? _bottomRight;

        /// <summary>
        /// Gets the thickness information that defines the fixed border sizes.
        /// </summary>
        public Thickness Info => _info;

        /// <summary>
        /// Initializes a new instance of the <see cref="NinePatchRegion"/> class.
        /// </summary>
        /// <param name="texture">The texture containing the nine-patch graphic.</param>
        /// <param name="bounds">The full rectangle of the nine-patch texture.</param>
        /// <param name="info">The border thickness defining the nine-patch scaling areas.</param>
        public NinePatchRegion(Texture2D texture, Rectangle bounds, Thickness info)
            : base(texture, bounds)
        {
            _info = info;

            int centerWidth = Math.Max(0, bounds.Width - info.Left - info.Right);
            int centerHeight = Math.Max(0, bounds.Height - info.Top - info.Bottom);

            int xLeft = bounds.X;
            int xCenter = bounds.X + info.Left;
            int xRight = bounds.X + info.Left + centerWidth;

            int yTop = bounds.Y;
            int yCenter = bounds.Y + info.Top;
            int yBottom = bounds.Y + info.Top + centerHeight;

            // Top row
            if (info.Top > 0)
            {
                if (info.Left > 0)
                    _topLeft = new TextureRegion(texture, new Rectangle(xLeft, yTop, info.Left, info.Top));

                if (centerWidth > 0)
                    _topCenter = new TextureRegion(texture, new Rectangle(xCenter, yTop, centerWidth, info.Top));

                if (info.Right > 0)
                    _topRight = new TextureRegion(texture, new Rectangle(xRight, yTop, info.Right, info.Top));
            }

            // Middle row
            if (centerHeight > 0)
            {
                if (info.Left > 0)
                    _centerLeft = new TextureRegion(texture, new Rectangle(xLeft, yCenter, info.Left, centerHeight));

                if (centerWidth > 0)
                    _center = new TextureRegion(texture, new Rectangle(xCenter, yCenter, centerWidth, centerHeight));

                if (info.Right > 0)
                    _centerRight = new TextureRegion(texture, new Rectangle(xRight, yCenter, info.Right, centerHeight));
            }

            // Bottom row
            if (info.Bottom > 0)
            {
                if (info.Left > 0)
                    _bottomLeft = new TextureRegion(texture, new Rectangle(xLeft, yBottom, info.Left, info.Bottom));

                if (centerWidth > 0)
                    _bottomCenter = new TextureRegion(texture, new Rectangle(xCenter, yBottom, centerWidth, info.Bottom));

                if (info.Right > 0)
                    _bottomRight = new TextureRegion(texture, new Rectangle(xRight, yBottom, info.Right, info.Bottom));
            }
        }

        /// <summary>
        /// Draws the nine-patch texture to a destination rectangle, scaling the center and edges while preserving the corners.
        /// </summary>
        /// <param name="context">The render context used for drawing.</param>
        /// <param name="dest">The destination rectangle.</param>
        /// <param name="color">The tint color to apply.</param>
        public override void Draw(RenderContext context, Rectangle dest, Color color)
        {
            // Clamp patch sizes to destination bounds
            int left = Math.Min(_info.Left, dest.Width);
            int right = Math.Min(_info.Right, dest.Width);
            int top = Math.Min(_info.Top, dest.Height);
            int bottom = Math.Min(_info.Bottom, dest.Height);

            int centerWidth = Math.Max(0, dest.Width - left - right);
            int centerHeight = Math.Max(0, dest.Height - top - bottom);

            int xLeft = dest.X;
            int xCenter = dest.X + left;
            int xRight = dest.X + left + centerWidth;

            int yTop = dest.Y;
            int yCenter = dest.Y + top;
            int yBottom = dest.Y + top + centerHeight;

            // Top row
            _topLeft?.Draw(context, new Rectangle(xLeft, yTop, left, top), color);
            if (centerWidth > 0)
                _topCenter?.Draw(context, new Rectangle(xCenter, yTop, centerWidth, top), color);
            _topRight?.Draw(context, new Rectangle(xRight, yTop, right, top), color);

            // Middle row
            if (centerHeight > 0)
            {
                _centerLeft?.Draw(context, new Rectangle(xLeft, yCenter, left, centerHeight), color);
                if (centerWidth > 0)
                    _center?.Draw(context, new Rectangle(xCenter, yCenter, centerWidth, centerHeight), color);
                _centerRight?.Draw(context, new Rectangle(xRight, yCenter, right, centerHeight), color);
            }

            // Bottom row
            _bottomLeft?.Draw(context, new Rectangle(xLeft, yBottom, left, bottom), color);
            if (centerWidth > 0)
                _bottomCenter?.Draw(context, new Rectangle(xCenter, yBottom, centerWidth, bottom), color);
            _bottomRight?.Draw(context, new Rectangle(xRight, yBottom, right, bottom), color);
        }
    }
}
