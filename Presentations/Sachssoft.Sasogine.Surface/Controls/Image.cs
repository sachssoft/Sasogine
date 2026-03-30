using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Image : Widget, IPressable
{
    private StyleProperty<ITextureRegion?> _visual = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<ITextureRegion?> _hoveredVisual = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<ITextureRegion?> _pressedVisual = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<Color> _color = new StyleProperty<Color>(Microsoft.Xna.Framework.Color.White, isUserSet: false);
    private StyleProperty<ImageResizeMode> _resizeMode = new StyleProperty<ImageResizeMode>(ImageResizeMode.Stretch, isUserSet: false);

    public Image() { }

    #region Style Properties

    public StyleProperty<ITextureRegion?> Visual
    {
        get => _visual;
        set
        {
            if (SetAndNotify(ref _visual, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<ITextureRegion?> HoveredVisual
    {
        get => _hoveredVisual;
        set
        {
            if (SetAndNotify(ref _hoveredVisual, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<ITextureRegion?> PressedVisual
    {
        get => _pressedVisual;
        set
        {
            if (SetAndNotify(ref _pressedVisual, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<Color> Color
    {
        get => _color;
        set => SetAndNotify(ref _color, value);
    }

    public StyleProperty<ImageResizeMode> ResizeMode
    {
        get => _resizeMode;
        set => SetAndNotify(ref _resizeMode, value);
    }

    #endregion

    internal bool IsPressed { get; set; }

    bool IPressable.IsPressed
    {
        get => IsPressed;
        set => IsPressed = value;
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        Point maxSize = new Point(0, 0);

        void ConsiderSize(Point s)
        {
            if (s.X > maxSize.X) maxSize.X = s.X;
            if (s.Y > maxSize.Y) maxSize.Y = s.Y;
        }

        if (_visual.Value != null) ConsiderSize(_visual.Value.Size);
        if (_hoveredVisual.Value != null) ConsiderSize(_hoveredVisual.Value.Size);
        if (_pressedVisual.Value != null) ConsiderSize(_pressedVisual.Value.Size);

        return maxSize;
    }

    public override void InternalRender(RenderContext context, GameTime t)
    {
        var image = Visual;

        if (IsMouseInside && HoveredVisual != null)
        {
            image = HoveredVisual;
        }

        if (IsPressed && PressedVisual != null)
        {
            image = PressedVisual;
        }

        if (image.Value != null)
        {
            var bounds = ActualBounds;

            if (ResizeMode == ImageResizeMode.KeepAspectRatio && image.Value.Size.Y != 0)
            {
                var aspect = (float)image.Value.Size.X / image.Value.Size.Y;
                bounds.Height = (int)(bounds.Width / aspect);
            }

            image.Value.Draw(context, bounds, Color);
        }
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(Visual):
                    target.Visual = target.Visual.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(HoveredVisual):
                    target.HoveredVisual = target.HoveredVisual.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(PressedVisual):
                    target.PressedVisual = target.PressedVisual.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(Color):
                    target.Color = target.Color.Override(value.ConvertTo<Color>());
                    break;
                case nameof(ResizeMode):
                    target.ResizeMode = target.ResizeMode.Override(value.ConvertTo<ImageResizeMode>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Image source)
            return;

        Visual = source.Visual;
        HoveredVisual = source.HoveredVisual;
        PressedVisual = source.PressedVisual;
        Color = source.Color;
        ResizeMode = source.ResizeMode;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Image();
    }

    #endregion
}
