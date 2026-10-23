using Sachssoft.Engine.Graphics.Colors;
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Input;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Interactive HSV area, hue strip and alpha strip, composed from the skin's independent gradients.
/// </summary>
public sealed class ColorPlane : Control
{
    private float _h, _s, _v = 1, _a = 1;
    private Color _observed = Color.White;
    private int _drag;
    /// <summary>
    /// Initializes a new color plane instance.
    /// </summary>
    public ColorPlane() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ColorPlane";

    /// <summary>
    /// Gets or sets selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => ConfigSelectedColor;
        set
        {
            ConfigSelectedColor = value;
            ReadColor(value);
            if (_observed != value)
            {
                _observed = value;
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Occurs when color changed.
    /// </summary>
    public event EventHandler? ColorChanged;
    private void ReadColor(Color color)
    {
        HsvColor hsv = HsvColor.FromColor(color);
        _s = hsv.Saturation;
        _v = hsv.Value;
        _a = hsv.Alpha;
        if (_s > 0)
            _h = hsv.Hue / 360;
    }

    internal static Color FromHsv(float hue, float saturation, float value, float alpha) => new HsvColor(hue * 360, saturation, value, alpha).ToColor();
    private Bounds2 Plane => new(Padding.Left, Padding.Top, MathF.Max(1, ContentBounds.Width - 30), MathF.Max(1, ContentBounds.Height - 30));
    private Bounds2 Hue => new(Plane.Right + 8, Plane.Y, 16, Plane.Height);
    private Bounds2 Alpha => new(Plane.X, Plane.Bottom + 8, Plane.Width, 16);

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(250, 230);
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_observed != SelectedColor)
        {
            ReadColor(SelectedColor);
            _observed = SelectedColor;
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetPoint(Vector2 absolute)
    {
        float x = absolute.X - Bounds.X, y = absolute.Y - Bounds.Y;
        if (_drag == 1)
        {
            _s = Math.Clamp((x - Plane.X) / Plane.Width, 0, 1);
            _v = 1 - Math.Clamp((y - Plane.Y) / Plane.Height, 0, 1);
        }
        else if (_drag == 2)
            _h = Math.Clamp((y - Hue.Y) / Hue.Height, 0, .999999f);
        else if (_drag == 3)
            _a = Math.Clamp((x - Alpha.X) / Alpha.Width, 0, 1);
        Color next = FromHsv(_h, _s, _v, _a);
        ConfigSelectedColor = next;
        if (next != _observed)
        {
            _observed = next;
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            float x = e.Position.X - Bounds.X, y = e.Position.Y - Bounds.Y;
            _drag = Contains(Plane, x, y) ? 1 : Contains(Hue, x, y) ? 2 : Contains(Alpha, x, y) ? 3 : 0;
            if (_drag == 0)
                return;
            context.CapturePointer(this);
            SetPoint(e.Position);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerMove && _drag != 0)
        {
            SetPoint(e.Position);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && _drag != 0)
        {
            context.ReleasePointer(this);
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => _drag = 0;
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        context.FillRectangle(Plane, FromHsv(_h, 1, 1, 1));
        context.DrawSkinRegion("color_saturation_overlay", Plane);
        context.DrawSkinRegion("color_value_overlay", Plane);
        context.DrawSkinRegion("color_hue_vertical", Hue);
        context.DrawSkinRegion("color_checker", Alpha);
        context.DrawSkinRegion("color_alpha_ramp", Alpha, new Color(SelectedColor.R, SelectedColor.G, SelectedColor.B, (byte)255));
        context.DrawSkinRegion("color_sv_cursor", new Bounds2(Plane.X + Plane.Width * _s - 8, Plane.Y + Plane.Height * (1 - _v) - 8, 16, 16));
        context.DrawSkinRegion("color_hue_cursor_vertical", new Bounds2(Hue.X - 3, Hue.Y + Hue.Height * _h - 3, 22, 6));
        context.StrokeRectangle(new Bounds2(Alpha.X + Alpha.Width * _a - 1, Alpha.Y, 2, Alpha.Height), Color.White, 1);
    }

    private StyleProperty<Color> _styleSelectedColor = new(Color.White);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedColorChangedArgs = new(nameof(SelectedColor));
    internal Color ConfigSelectedColor
    {
        get => _styleSelectedColor.Value;
        set
        {
            if (_styleSelectedColor.SetLocal(value))
                NotifyPropertyChanged(SelectedColorChangedArgs);
        }
    }

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "SelectedColor":
                    if (_styleSelectedColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(SelectedColorChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "SelectedColor":
                return _styleSelectedColor.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "SelectedColor":
                if (_styleSelectedColor.ClearLocal())
                    NotifyPropertyChanged(SelectedColorChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleSelectedColor.BeginStyle(Color.White))
            NotifyPropertyChanged(SelectedColorChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSelectedColor.CommitStyle())
            NotifyPropertyChanged(SelectedColorChangedArgs);
    }
}
