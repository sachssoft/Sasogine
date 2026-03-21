using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions;

/// <summary>
/// Represents a nine-patch image region that divides a texture into nine sections
/// and draws them stretched/scaled to fit a given destination rectangle.
/// Implements <see cref="IImage"/> for size querying and drawing.
/// </summary>
public sealed class NinePatchRegion : ITextureRegion
{
    private readonly TextureRegion _topLeft;
    private readonly TextureRegion _topCenter;
    private readonly TextureRegion _topRight;

    private readonly TextureRegion _middleLeft;
    private readonly TextureRegion _center;
    private readonly TextureRegion _middleRight;

    private readonly TextureRegion _bottomLeft;
    private readonly TextureRegion _bottomCenter;
    private readonly TextureRegion _bottomRight;

    /// <summary>
    /// Initializes a new instance of the <see cref="NinePatchRegion"/> class
    /// from explicit nine texture regions.
    /// </summary>
    /// <param name="topLeft">Top-left region.</param>
    /// <param name="topCenter">Top-center region.</param>
    /// <param name="topRight">Top-right region.</param>
    /// <param name="middleLeft">Middle-left region.</param>
    /// <param name="center">Center region.</param>
    /// <param name="middleRight">Middle-right region.</param>
    /// <param name="bottomLeft">Bottom-left region.</param>
    /// <param name="bottomCenter">Bottom-center region.</param>
    /// <param name="bottomRight">Bottom-right region.</param>
    public NinePatchRegion(
        TextureRegion topLeft, TextureRegion topCenter, TextureRegion topRight,
        TextureRegion middleLeft, TextureRegion center, TextureRegion middleRight,
        TextureRegion bottomLeft, TextureRegion bottomCenter, TextureRegion bottomRight)
    {
        _topLeft = topLeft ?? throw new ArgumentNullException(nameof(topLeft));
        _topCenter = topCenter ?? throw new ArgumentNullException(nameof(topCenter));
        _topRight = topRight ?? throw new ArgumentNullException(nameof(topRight));

        _middleLeft = middleLeft ?? throw new ArgumentNullException(nameof(middleLeft));
        _center = center ?? throw new ArgumentNullException(nameof(center));
        _middleRight = middleRight ?? throw new ArgumentNullException(nameof(middleRight));

        _bottomLeft = bottomLeft ?? throw new ArgumentNullException(nameof(bottomLeft));
        _bottomCenter = bottomCenter ?? throw new ArgumentNullException(nameof(bottomCenter));
        _bottomRight = bottomRight ?? throw new ArgumentNullException(nameof(bottomRight));

        InitProperties(
            new Rectangle(
                _topLeft.Bounds.X,
                _topLeft.Bounds.Y,
                _topLeft.Bounds.Width + _topCenter.Bounds.Width + _topRight.Bounds.Width,
                _topLeft.Bounds.Height + _middleLeft.Bounds.Height + _bottomLeft.Bounds.Height
            ),
            _topLeft.Bounds.Width,   // Left
            _topLeft.Bounds.Height,  // Top
            _topRight.Bounds.Width,  // Right
            _bottomLeft.Bounds.Height // Bottom
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NinePatchRegion"/> class
    /// by slicing a texture using given bounds and inset values.
    /// </summary>
    /// <param name="texture">Source texture.</param>
    /// <param name="bounds">The region within the texture to slice.</param>
    /// <param name="left">Inset from the left edge.</param>
    /// <param name="top">Inset from the top edge.</param>
    /// <param name="right">Inset from the right edge.</param>
    /// <param name="bottom">Inset from the bottom edge.</param>
    public NinePatchRegion(Texture2D texture, Rectangle bounds, int left, int top, int right, int bottom)
    {
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        if (left < 0 || top < 0 || right < 0 || bottom < 0) throw new ArgumentException("Insets must be non-negative.");
        if (left + right > bounds.Width || top + bottom > bounds.Height) throw new ArgumentException("Insets are larger than bounds.");

        int centerWidth = Math.Max(0, bounds.Width - left - right);
        int centerHeight = Math.Max(0, bounds.Height - top - bottom);

        int xLeft = bounds.X;
        int xCenter = bounds.X + left;
        int xRight = bounds.X + left + centerWidth;

        int yTop = bounds.Y;
        int yCenter = bounds.Y + top;
        int yBottom = bounds.Y + top + centerHeight;

        _topLeft = new TextureRegion(texture, new Rectangle(xLeft, yTop, left, top));
        _topCenter = new TextureRegion(texture, new Rectangle(xCenter, yTop, centerWidth, top));
        _topRight = new TextureRegion(texture, new Rectangle(xRight, yTop, right, top));

        _middleLeft = new TextureRegion(texture, new Rectangle(xLeft, yCenter, left, centerHeight));
        _center = new TextureRegion(texture, new Rectangle(xCenter, yCenter, centerWidth, centerHeight));
        _middleRight = new TextureRegion(texture, new Rectangle(xRight, yCenter, right, centerHeight));

        _bottomLeft = new TextureRegion(texture, new Rectangle(xLeft, yBottom, left, bottom));
        _bottomCenter = new TextureRegion(texture, new Rectangle(xCenter, yBottom, centerWidth, bottom));
        _bottomRight = new TextureRegion(texture, new Rectangle(xRight, yBottom, right, bottom));

        InitProperties(bounds, left, top, right, bottom);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NinePatchRegion"/> class
    /// by slicing the entire texture using given inset values.
    /// </summary>
    /// <param name="texture">Source texture.</param>
    /// <param name="left">Inset from the left edge.</param>
    /// <param name="top">Inset from the top edge.</param>
    /// <param name="right">Inset from the right edge.</param>
    /// <param name="bottom">Inset from the bottom edge.</param>
    public NinePatchRegion(Texture2D texture, int left, int top, int right, int bottom)
        : this(texture, new Rectangle(0, 0, texture.Width, texture.Height), left, top, right, bottom)
    { }

    /// <summary>
    /// Inset vom linken Rand.
    /// </summary>
    public int Left { get; private set; }

    /// <summary>
    /// Inset vom oberen Rand.
    /// </summary>
    public int Top { get; private set; }

    /// <summary>
    /// Inset vom rechten Rand.
    /// </summary>
    public int Right { get; private set; }

    /// <summary>
    /// Inset vom unteren Rand.
    /// </summary>
    public int Bottom { get; private set; }

    /// <summary>
    /// Original-Bounds des Nine-Patch im Texture.
    /// </summary>
    public Rectangle Bounds { get; private set; }

    public RegionOptions? Options { get; set; }

    /// <summary>
    /// Gets the combined size of the original nine-patch regions (sum of widths and heights).
    /// </summary>
    public Point Size
    {
        get
        {
            int width = _topLeft.Bounds.Width + _topCenter.Bounds.Width + _topRight.Bounds.Width;
            int height = _topLeft.Bounds.Height + _middleLeft.Bounds.Height + _bottomLeft.Bounds.Height;
            return new Point(width, height);
        }
    }    
    
    // Diese Properties müssen einmal nach Konstruktion gesetzt werden
    // z.B. im Konstruktor mit Texture + Insets:
    private void InitProperties(Rectangle bounds, int left, int top, int right, int bottom)
    {
        Bounds = bounds;
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    /// Draws the nine-patch image stretched to fit the specified destination rectangle.
    /// The corner regions keep their original size; edges and center are stretched accordingly.
    /// </summary>
    /// <param name="context">Rendering context.</param>
    /// <param name="dest">Destination rectangle to draw into.</param>
    /// <param name="color">Tint color.</param>
    public void Draw(RenderContext context, Rectangle dest, Color color)
    {
        int leftWidth = _topLeft.Bounds.Width;
        int rightWidth = _topRight.Bounds.Width;
        int topHeight = _topLeft.Bounds.Height;
        int bottomHeight = _bottomLeft.Bounds.Height;

        int centerWidth = Math.Max(0, dest.Width - leftWidth - rightWidth);
        int centerHeight = Math.Max(0, dest.Height - topHeight - bottomHeight);

        // Draw top row
        _topLeft.Draw(context, new Rectangle(dest.X, dest.Y, leftWidth, topHeight), color);
        _topCenter.Draw(context, new Rectangle(dest.X + leftWidth, dest.Y, centerWidth, topHeight), color);
        _topRight.Draw(context, new Rectangle(dest.X + leftWidth + centerWidth, dest.Y, rightWidth, topHeight), color);

        // Draw middle row
        _middleLeft.Draw(context, new Rectangle(dest.X, dest.Y + topHeight, leftWidth, centerHeight), color);
        _center.Draw(context, new Rectangle(dest.X + leftWidth, dest.Y + topHeight, centerWidth, centerHeight), color);
        _middleRight.Draw(context, new Rectangle(dest.X + leftWidth + centerWidth, dest.Y + topHeight, rightWidth, centerHeight), color);

        // Draw bottom row
        _bottomLeft.Draw(context, new Rectangle(dest.X, dest.Y + topHeight + centerHeight, leftWidth, bottomHeight), color);
        _bottomCenter.Draw(context, new Rectangle(dest.X + leftWidth, dest.Y + topHeight + centerHeight, centerWidth, bottomHeight), color);
        _bottomRight.Draw(context, new Rectangle(dest.X + leftWidth + centerWidth, dest.Y + topHeight + centerHeight, rightWidth, bottomHeight), color);
    }
}
