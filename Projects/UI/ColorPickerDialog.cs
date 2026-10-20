using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics;
using Sachssoft.Engine.Graphics.Colors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>Color-plane, RGBA/hex and Sasogine RGB, normalized RGB, HSL, HSV, linear RGB and OKLab editing.</summary>
public sealed class ColorPickerDialog : Dialog
{
    private readonly ColorPlane _plane = new();
    private readonly TextBox[] _channels =
    {
        new(),
        new(),
        new(),
        new()
    };
    private readonly ReadOnlyCollection<TextBox> _channelView;
    private readonly string[] _labels =
    {
        "R",
        "G",
        "B",
        "A"
    };
    private readonly ReadOnlyCollection<string> _labelView;
    private readonly TextBox _hex = new();
    private readonly ComboBox _models = new()
    {
        Height = 32
    };
    private readonly Button _ok = new()
    {
        Content = new TextBlock
        {
            Text = "OK"
        }
    }, _cancel = new()
    {
        Content = new TextBlock
        {
            Text = "Cancel"
        }
    };
    private ChangeTracker<Color> _color;
    private ChangeTracker<ColorPickerModel> _model;
    private bool _syncing;
    /// <summary>Creates the color editor.</summary>
    public ColorPickerDialog() : base()
    {
        _channelView = Array.AsReadOnly(_channels);
        _labelView = Array.AsReadOnly(_labels);
        foreach (string name in new[]
        {
            "RGBA (0–255)",
            "RGB (0–1)",
            "Normalized RGBA",
            "HSL",
            "HSV",
            "Linear RGB",
            "OKLab"
        }

        )
            _models.Items.Add(new SelectorItem(name));
        _models.SelectionChanged += (_, _) =>
        {
            if (!_syncing && _models.SelectedIndex >= 0)
            {
                ConfigColorModel = (ColorPickerModel)_models.SelectedIndex;
                SynchronizeModel();
            }
        };
        _plane.ColorChanged += (_, _) =>
        {
            if (!_syncing)
                SelectedColor = _plane.SelectedColor;
        };
        foreach (TextBox channel in _channels)
            channel.TextChanged += (_, _) => EditChannels();
        _hex.TextChanged += (_, _) => EditHex();
        _ok.Clicked += (_, _) => OnAccept();
        _cancel.Clicked += (_, _) => Close();
        DefaultButton = _ok;
        PickButton.Clicked += (_, _) => BeginColorPick();
        Content = new ColorPickerLayout(this, _plane, _models, _channels, _hex, _ok, _cancel, PickButton);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ColorPickerDialog";

    /// <summary>Gets or sets the selected color. Components use the engine's straight RGBA representation.</summary>
    public Color SelectedColor
    {
        get
        {
            return ConfigSelectedColor;
        }

        set
        {
            ConfigSelectedColor = value;
            ApplyColor();
        }
    }

    /// <summary>Gets or sets the active numeric color representation.</summary>
    public ColorPickerModel ColorModel
    {
        get => ConfigColorModel;
        set
        {
            ConfigColorModel = value;
            SynchronizeModel();
        }
    }

    /// <summary>Gets the render-target pipette button; enabled when a source is available.</summary>
    public Button PickButton { get; } = new() { Content = new TextBlock { Text = "Pick pixel" }, ToolTip = "Pick a render-target pixel; Escape cancels" };
    /// <summary>Gets or sets an optional source overriding the workspace source.</summary>
    public IColorPickSource? PickSource { get; set; }
    /// <summary>Starts an input-isolated viewport pick with rollback on Escape.</summary>
    public void BeginColorPick()
    {
        Workspace? owner = Workspace;
        IColorPickSource? source = PickSource ?? owner?.ColorPickSource;
        if (owner is null || source is null || owner.ColorPicking.IsActive) return;
        Color original = SelectedColor;
        owner.ColorPicking.Begin(this, source, original, value => SelectedColor = value,
            value => SelectedColor = value, () => SelectedColor = original);
    }
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        PickButton.IsEnabled = (PickSource ?? Workspace?.ColorPickSource) is not null;
    }
    /// <inheritdoc/>
    protected override void OnDetached()
    {
        if (Workspace?.ColorPicking.Owner == this) Workspace.ColorPicking.Cancel();
        base.OnDetached();
    }

    /// <summary>Gets the active model selector.</summary>
    public ComboBox ModelSelector => _models;
    /// <summary>Gets the four editors for the selected model.</summary>
    public IReadOnlyList<TextBox> ChannelInputs => _channelView;
    /// <summary>Gets model-specific channel labels.</summary>
    public IReadOnlyList<string> ChannelLabels => _labelView;
    /// <summary>Gets the RGBA hexadecimal editor.</summary>
    public TextBox HexInput => _hex;
    /// <summary>Gets the interactive HSV plane.</summary>
    public ColorPlane ColorEditor => _plane;

    /// <summary>Occurs once when the color changes.</summary>
    public event EventHandler? ColorChanged;
    internal override void Prepare()
    {
        base.Prepare();
        SynchronizeModel();
        ApplyColor();
    }

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        SynchronizeModel();
        ApplyColor();
        SyncFields(true);
    }

    private void SynchronizeModel()
    {
        if (ConfigColorModel is not (ColorPickerModel.Rgba or ColorPickerModel.Rgb or ColorPickerModel.NormalizedRgb or ColorPickerModel.Hsl or ColorPickerModel.Hsv or ColorPickerModel.LinearRgb or ColorPickerModel.Oklab))
            throw new InvalidOperationException("Unknown color picker model.");
        if (!_model.EnsureChange(ConfigColorModel))
            return;
        bool old = _syncing;
        _syncing = true;
        _models.SelectedIndex = (int)ConfigColorModel;
        _syncing = old;
        SyncFields(true);
    }

    private void ApplyColor()
    {
        Color next = ConfigSelectedColor;
        if (!_color.EnsureChange(next))
            return;
        bool old = _syncing;
        _syncing = true;
        _plane.SelectedColor = next;
        _syncing = old;
        SyncFields(false);
        ColorChanged?.Invoke(this, EventArgs.Empty);
    }

    private Vector4 Channels()
    {
        Color c = ConfigSelectedColor;
        switch (ColorModel)
        {
            case ColorPickerModel.Rgba:
                return new Vector4(c.R, c.G, c.B, c.A);
            case ColorPickerModel.Rgb:
                var rgb = RgbColor.FromColor(c);
                return new Vector4(rgb.Red, rgb.Green, rgb.Blue, rgb.Alpha);
            case ColorPickerModel.NormalizedRgb:
                var n = new NormalizedColor(c.ToVector4());
                return new Vector4(n.R, n.G, n.B, n.A);
            case ColorPickerModel.Hsl:
                var hsl = HslColor.FromColor(c);
                return new Vector4(hsl.Hue, hsl.Saturation * 100, hsl.Lightness * 100, hsl.Alpha * 100);
            case ColorPickerModel.Hsv:
                var hsv = HsvColor.FromColor(c);
                return new Vector4(hsv.Hue, hsv.Saturation * 100, hsv.Value * 100, hsv.Alpha * 100);
            case ColorPickerModel.LinearRgb:
                var linear = LinearRgbColor.FromColor(c);
                return new Vector4(linear.Red, linear.Green, linear.Blue, linear.Alpha);
            case ColorPickerModel.Oklab:
                var lab = OklabColor.FromColor(c);
                return new Vector4(lab.Lightness, lab.A, lab.B, lab.Alpha);
            default:
                throw new InvalidOperationException("Unknown color model.");
        }
    }

    private static float Component(Vector4 value, int index) => index switch
    {
        0 => value.X,
        1 => value.Y,
        2 => value.Z,
        _ => value.W
    };
    private void SyncFields(bool force)
    {
        bool old = _syncing;
        _syncing = true;
        try
        {
            Vector4 channels = Channels();
            _labels[0] = ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv ? "H °" : ColorModel == ColorPickerModel.Oklab ? "L" : "R";
            _labels[1] = ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv ? "S %" : ColorModel == ColorPickerModel.Oklab ? "a" : "G";
            _labels[2] = ColorModel == ColorPickerModel.Hsl ? "L %" : ColorModel == ColorPickerModel.Hsv ? "V %" : ColorModel == ColorPickerModel.Oklab ? "b" : "B";
            _labels[3] = ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv ? "A %" : "A";
            for (int i = 0; i < 4; i++)
                if (force || !_channels[i].IsFocused)
                {
                    _channels[i].Text = Component(channels, i).ToString(ColorModel == ColorPickerModel.Rgba ? "0" : "0.#####", CultureInfo.InvariantCulture);
                    _channels[i].HasError = false;
                }

            if (force || !_hex.IsFocused)
            {
                Color c = ConfigSelectedColor;
                _hex.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
                _hex.HasError = false;
            }
        }
        finally
        {
            _syncing = old;
        }
    }

    private bool WithinRange(float value, int channel)
    {
        if (!float.IsFinite(value))
            return false;
        if (ColorModel == ColorPickerModel.Rgba)
            return value >= 0 && value <= 255 && value == MathF.Truncate(value);
        if (ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv)
            return value >= 0 && value <= (channel == 0 ? 360 : 100);
        if (ColorModel == ColorPickerModel.Oklab && channel is 1 or 2)
            return value >= -.5f && value <= .5f;
        return value >= 0 && value <= 1;
    }

    private void EditChannels()
    {
        if (_syncing)
            return;
        Span<float> values = stackalloc float[4];
        bool valid = true;
        for (int i = 0; i < 4; i++)
        {
            bool okay = float.TryParse(_channels[i].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]) && WithinRange(values[i], i);
            _channels[i].HasError = !okay;
            valid &= okay;
        }

        if (!valid)
            return;
        float a = values[0], b = values[1], c = values[2], alpha = values[3];
        SelectedColor = ColorModel switch
        {
            ColorPickerModel.Rgba => new Color((byte)a, (byte)b, (byte)c, (byte)alpha),
            ColorPickerModel.Rgb => new RgbColor(a, b, c, alpha).ToColor(),
            ColorPickerModel.NormalizedRgb => new NormalizedColor(a, b, c, alpha).ToColor(),
            ColorPickerModel.Hsl => new HslColor(a, b / 100, c / 100, alpha / 100).ToColor(),
            ColorPickerModel.Hsv => new HsvColor(a, b / 100, c / 100, alpha / 100).ToColor(),
            ColorPickerModel.LinearRgb => new LinearRgbColor(a, b, c, alpha).ToColor(),
            ColorPickerModel.Oklab => new OklabColor(a, b, c, alpha).ToColor(),
            _ => throw new InvalidOperationException("Unknown color model.")};
    }

    private void EditHex()
    {
        if (_syncing)
            return;
        string text = _hex.Text.TrimStart('#');
        if (text.Length is not (6 or 8) || !uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint packed))
        {
            _hex.HasError = true;
            return;
        }

        _hex.HasError = false;
        SelectedColor = text.Length == 6 ? new Color((byte)((packed >> 16) & 255), (byte)((packed >> 8) & 255), (byte)(packed & 255), (byte)255) : new Color((byte)(packed >> 24), (byte)((packed >> 16) & 255), (byte)((packed >> 8) & 255), (byte)(packed & 255));
    }

    private bool ValidFields()
    {
        foreach (TextBox input in _channels)
            if (input.HasError)
                return false;
        return !_hex.HasError;
    }

    /// <inheritdoc/>
    protected override void OnAccept()
    {
        if (ValidFields())
            Complete(DialogResult.OK);
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

    private StyleProperty<ColorPickerModel> _styleColorModel = new(ColorPickerModel.Rgba);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColorModelChangedArgs = new(nameof(ColorModel));
    internal ColorPickerModel ConfigColorModel
    {
        get => _styleColorModel.Value;
        set
        {
            if (_styleColorModel.SetLocal(value))
                NotifyPropertyChanged(ColorModelChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
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
                case "ColorModel":
                    if (_styleColorModel.Apply(Converters.Convert<ColorPickerModel>("ColorPickerModel", entry.Value)))
                        NotifyPropertyChanged(ColorModelChangedArgs);
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
            case "ColorModel":
                return _styleColorModel.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "SelectedColor":
                if (_styleSelectedColor.ClearLocal())
                    NotifyPropertyChanged(SelectedColorChangedArgs);
                return;
            case "ColorModel":
                if (_styleColorModel.ClearLocal())
                    NotifyPropertyChanged(ColorModelChangedArgs);
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
        if (_styleColorModel.BeginStyle(ColorPickerModel.Rgba))
            NotifyPropertyChanged(ColorModelChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSelectedColor.CommitStyle())
            NotifyPropertyChanged(SelectedColorChangedArgs);
        if (_styleColorModel.CommitStyle())
            NotifyPropertyChanged(ColorModelChangedArgs);
    }
}
