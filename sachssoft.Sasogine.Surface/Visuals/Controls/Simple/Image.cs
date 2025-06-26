using Microsoft.Xna.Framework;
using System.ComponentModel;
using sachssoft.Sasogine.Surface.Utility;
using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public enum ImageResizeMode
{
    /// <summary>
    /// Simply Stretch
    /// </summary>
    Stretch,

    /// <summary>
    /// Keep Aspect Ratio
    /// </summary>
    KeepAspectRatio
}

internal interface IPressable
{
    bool IsPressed { get; set; }
}

public class Image : Widget, IPressable
{
    private IImage? _image;
    private IImage? _overImage;
    private IImage? _pressedImage;

    public IImage? Renderable
    {
        get => _image;
        set
        {
            if (value == _image) return;
            _image = value;
            InvalidateMeasure();
        }
    }

    public IImage? OverRenderable
    {
        get => _overImage;
        set
        {
            if (value == _overImage) return;
            _overImage = value;
            InvalidateMeasure();
        }
    }

    public IImage? PressedRenderable
    {
        get => _pressedImage;
        set
        {
            if (value == _pressedImage) return;
            _pressedImage = value;
            InvalidateMeasure();
        }
    }

    internal bool IsPressed { get; set; }

    bool IPressable.IsPressed
    {
        get => IsPressed;
        set => IsPressed = value;
    }

    public Color Color { get; set; } = Color.White;

    public ImageResizeMode ResizeMode { get; set; } = ImageResizeMode.Stretch;

    protected override Point InternalMeasure(Point availableSize)
    {
        Point maxSize = new Point(0, 0);

        void ConsiderSize(Point s)
        {
            if (s.X > maxSize.X) maxSize.X = s.X;
            if (s.Y > maxSize.Y) maxSize.Y = s.Y;
        }

        if (_image != null) ConsiderSize(_image.Size);
        if (_overImage != null) ConsiderSize(_overImage.Size);
        if (_pressedImage != null) ConsiderSize(_pressedImage.Size);

        return maxSize;
    }

    public override void InternalRender(RenderContext context)
    {
        var image = Renderable;

        if (IsMouseInside && OverRenderable != null)
        {
            image = OverRenderable;
        }

        if (IsPressed && PressedRenderable != null)
        {
            image = PressedRenderable;
        }

        if (image != null)
        {
            var bounds = ActualBounds;

            if (ResizeMode == ImageResizeMode.KeepAspectRatio && image.Size.Y != 0)
            {
                var aspect = (float)image.Size.X / image.Size.Y;
                bounds.Height = (int)(bounds.Width / aspect);
            }

            image.Draw(context, bounds, Color);
        }
    }

    public void ApplyPressableImageStyle(PressableImageStyle imageStyle)
    {
        ApplyWidgetStyle(imageStyle);

        Renderable = imageStyle.Image;
        OverRenderable = imageStyle.OverImage;
        PressedRenderable = imageStyle.PressedImage;
    }

    protected internal override void CopyFrom(Widget w)
    {
        base.CopyFrom(w);

        var image = (Image)w;

        Renderable = image.Renderable;
        OverRenderable = image.OverRenderable;
        PressedRenderable = image.PressedRenderable;
        Color = image.Color;
        ResizeMode = image.ResizeMode;
    }
}
