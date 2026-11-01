using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Color-plane, RGBA/hex and Sasogine RGB, normalized RGB, HSL, HSV, linear RGB and OKLab editing.
/// </summary>
public sealed class ColorPickerDialog : Dialog
{
    /// <summary>Identifies changes to the active color representation.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColorModelChangedArgs = new(nameof(ColorModel));
    /// <summary>Identifies changes to the selected plane channel.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedChannelChangedArgs = new(nameof(SelectedChannel));
    /// <summary>Identifies changes to straight RGBA.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedColorChangedArgs = new(nameof(SelectedColor));

    /// <summary>Stores the cancel action.</summary>
    private readonly Button _cancel = new() { Content = new TextBlock { Text = "Cancel" } };
    /// <summary>Stores the accept action.</summary>
    private readonly Button _ok = new() { Content = new TextBlock { Text = "OK" } };
    /// <summary>Stores the render-target pipette action.</summary>
    private readonly Button _pickButton = new() { Content = new TextBlock { Text = "Pick pixel" }, ToolTip = "Pick a render-target pixel; Escape cancels" };

    /// <summary>Stores the model-aware plane and channel strips.</summary>
    private readonly ColorPlane _plane = new();

    /// <summary>Stores the explicit model selector.</summary>
    private readonly ComboBox _models = new() { Height = 32 };

    /// <summary>Stores the three scoped channel selectors, excluding alpha.</summary>
    private readonly RadioButton[] _selectors = { new(), new(), new() };

    /// <summary>Exposes channel editors without exposing their backing array.</summary>
    private readonly ReadOnlyCollection<TextBox> _channelView;

    /// <summary>Exposes model-specific labels without exposing their backing array.</summary>
    private readonly ReadOnlyCollection<string> _labelView;

    /// <summary>Stores the hexadecimal RGBA editor.</summary>
    private readonly TextBox _hex = new();

    /// <summary>Stores the numeric editors for three model channels and alpha.</summary>
    private readonly TextBox[] _channels = { new(), new(), new(), new() };

    /// <summary>Stores the active model's captions.</summary>
    private readonly string[] _labels = { "R", "G", "B", "A" };

    /// <summary>Tracks effective straight-color changes.</summary>
    private ChangeTracker<Color> _color;

    /// <summary>Tracks effective model changes.</summary>
    private ChangeTracker<ColorPickerModel> _model;

    /// <summary>Stores straight RGBA with local-over-style precedence.</summary>
    private StyleProperty<Color> _styleSelectedColor = new(Color.White);

    /// <summary>Stores the representation with local-over-style precedence.</summary>
    private StyleProperty<ColorPickerModel> _styleColorModel = new(ColorPickerModel.Rgba);

    /// <summary>Stores the selected channel with local-over-style precedence.</summary>
    private StyleProperty<int> _styleSelectedChannel = new(0);

    /// <summary>Prevents feedback between plane, text, selectors, and model updates.</summary>
    private bool _syncing;

    /// <summary>Occurs once when straight RGBA changes.</summary>
    public event EventHandler? ColorChanged;

    /// <summary>
    /// Creates the color editor.
    /// </summary>
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
        _plane.CoordinatesChanged += (_, _) =>
        {
            if (!_syncing)
            {
                bool changed = SelectedColor != _plane.SelectedColor;
                SelectedColor = _plane.SelectedColor;
                if (!changed) SyncFields(false);
            }
        };
        foreach (TextBox channel in _channels)
            channel.TextChanged += (_, _) => EditChannels();
        for (int i = 0; i < _selectors.Length; i++)
        {
            int channel = i;
            _selectors[i].GroupName = string.Empty;
            _selectors[i].CheckedChanged += (_, _) =>
            {
                if (!_syncing && _selectors[channel].IsChecked == true) SelectedChannel = channel;
            };
        }
        _hex.TextChanged += (_, _) => EditHex();
        _ok.Clicked += (_, _) => OnAccept();
        _cancel.Clicked += (_, _) => Close();
        DefaultButton = _ok;
        PickButton.Clicked += (_, _) => BeginColorPick();
        Content = new ColorPickerLayout(this, _plane, _models, _selectors, _channels, _hex, _ok, _cancel, PickButton);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(ColorPickerDialog);

    /// <summary>
    /// Gets or sets the selected color. Components use the engine's straight RGBA representation.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the active numeric color representation.
    /// </summary>
    public ColorPickerModel ColorModel
    {
        get => ConfigColorModel;
        set
        {
            ConfigColorModel = value;
            SynchronizeModel();
        }
    }

    /// <summary>Gets or sets the zero-based model channel controlled by the vertical strip; the other two form the plane.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The channel is outside zero through two.</exception>
    public int SelectedChannel
    {
        get => _styleSelectedChannel.Value;
        set
        {
            if ((uint)value > 2) throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleSelectedChannel.SetLocal(value)) NotifyPropertyChanged(SelectedChannelChangedArgs);
            SynchronizePlane();
        }
    }

    /// <summary>
    /// Gets the render-target pipette button; enabled when a source is available.
    /// </summary>
    public Button PickButton => _pickButton;
    /// <summary>
    /// Gets or sets an optional source overriding the workspace source.
    /// </summary>
    public IColorPickSource? PickSource
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the active model selector.
    /// </summary>
    public ComboBox ModelSelector => _models;
    /// <summary>
    /// Gets the four editors for the selected model.
    /// </summary>
    public IReadOnlyList<TextBox> ChannelInputs => _channelView;
    /// <summary>
    /// Gets model-specific channel labels.
    /// </summary>
    public IReadOnlyList<string> ChannelLabels => _labelView;
    /// <summary>
    /// Gets the RGBA hexadecimal editor.
    /// </summary>
    public TextBox HexInput => _hex;
    /// <summary>
    /// Gets the interactive plane for the currently selected color model and channel.
    /// </summary>
    public ColorPlane ColorEditor => _plane;

    /// <summary>Gets or sets straight RGBA for the public property and style pipeline.</summary>
    internal Color ConfigSelectedColor
    {
        get => _styleSelectedColor.Value;
        set
        {
            if (_styleSelectedColor.SetLocal(value))
                NotifyPropertyChanged(SelectedColorChangedArgs);
        }
    }

    /// <summary>Gets or sets the representation used by the public property and style pipeline.</summary>
    internal ColorPickerModel ConfigColorModel
    {
        get => _styleColorModel.Value;
        set
        {
            if (_styleColorModel.SetLocal(value))
                NotifyPropertyChanged(ColorModelChangedArgs);
        }
    }

    /// <summary>
    /// Starts an input-isolated viewport pick with rollback on Escape.
    /// </summary>
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

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        SynchronizeModel();
        ApplyColor();
        SynchronizePlane();
    }

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        SynchronizeModel();
        ApplyColor();
        SyncFields(true);
    }

    /// <summary>Synchronizes model selection, captions, and plane after a representation change.</summary>
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
        SynchronizePlane();
        InvalidateVisual();
    }

    /// <summary>Synchronizes the channel selector group and the plane without modifying the selected color.</summary>
    private void SynchronizePlane()
    {
        bool old = _syncing;
        _syncing = true;
        try
        {
            _plane.ColorModel = ColorModel;
            _plane.SelectedChannel = SelectedChannel;
            for (int i = 0; i < _selectors.Length; i++) _selectors[i].IsChecked = i == SelectedChannel;
        }
        finally { _syncing = old; }
    }

    /// <summary>Synchronizes effective color changes and emits one notification.</summary>
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

    /// <summary>Converts straight RGBA to displayed numeric channels.</summary>
    /// <returns>The three display components and model-specific alpha.</returns>
    private Vector4 Channels()
    {
        Vector4 value = _plane.ColorModel == ColorModel && _plane.SelectedColor == SelectedColor
            ? _plane.ModelChannels : ColorPickerChannels.Read(SelectedColor, ColorModel);
        if (ColorModel == ColorPickerModel.Rgba) return value * 255f;
        if (ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv)
            return new Vector4(value.X * 360f, value.Y * 100f, value.Z * 100f, value.W * 100f);
        if (ColorModel == ColorPickerModel.Oklab)
        {
            value.Y -= 0.5f;
            value.Z -= 0.5f;
        }
        return value;
    }

    /// <summary>Reads a display component without boxing.</summary>
    /// <param name="value">The displayed channels.</param>
    /// <param name="index">The zero-based component index.</param>
    /// <returns>The requested component.</returns>
    private static float Component(Vector4 value, int index) => index switch
    {
        0 => value.X,
        1 => value.Y,
        2 => value.Z,
        _ => value.W
    };
    /// <summary>Synchronizes captions and numeric text without replacing focused edits unnecessarily.</summary>
    /// <param name="force">Whether focused editors should also be replaced.</param>
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
            for (int i = 0; i < _selectors.Length; i++) _selectors[i].Content = _labels[i];
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

    /// <summary>Checks the limits for a channel in the active representation.</summary>
    /// <param name="value">The parsed value.</param>
    /// <param name="channel">The zero-based channel index.</param>
    /// <returns>Whether the value belongs to the supported range.</returns>
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

    /// <summary>Applies valid numeric channel edits to straight RGBA.</summary>
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
        var normalized = new Vector4(values[0], values[1], values[2], values[3]);
        if (ColorModel == ColorPickerModel.Rgba) normalized /= 255f;
        else if (ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv)
            normalized = new Vector4(normalized.X / 360f, normalized.Y / 100f, normalized.Z / 100f, normalized.W / 100f);
        else if (ColorModel == ColorPickerModel.Oklab)
        {
            normalized.Y += 0.5f;
            normalized.Z += 0.5f;
        }
        SynchronizePlane();
        _plane.SetModelChannels(normalized);
    }

    /// <summary>Applies valid RGB or RGBA hexadecimal text.</summary>
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

    /// <summary>Checks whether all channel and hexadecimal inputs can be accepted.</summary>
    /// <returns>Whether every editor contains a valid value.</returns>
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

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    /// <param name="style">The model, color, selected-channel, and inherited visual style values.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(SelectedColor):
                    if (_styleSelectedColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(SelectedColorChangedArgs);
                    break;
                case nameof(ColorModel):
                    if (_styleColorModel.Apply(Converters.Convert<ColorPickerModel>(nameof(ColorPickerModel), entry.Value)))
                        NotifyPropertyChanged(ColorModelChangedArgs);
                    break;
                case nameof(SelectedChannel):
                    int channel = Converters.Convert<int>("Int32", entry.Value);
                    if ((uint)channel > 2) throw new ArgumentOutOfRangeException(nameof(style));
                    if (_styleSelectedChannel.Apply(channel)) NotifyPropertyChanged(SelectedChannelChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case nameof(SelectedColor):
                return _styleSelectedColor.HasLocalValue;
            case nameof(ColorModel):
                return _styleColorModel.HasLocalValue;
            case nameof(SelectedChannel):
                return _styleSelectedChannel.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    /// <param name="property">The property whose local override is removed.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(SelectedColor):
                if (_styleSelectedColor.ClearLocal())
                    NotifyPropertyChanged(SelectedColorChangedArgs);
                return;
            case nameof(ColorModel):
                if (_styleColorModel.ClearLocal())
                    NotifyPropertyChanged(ColorModelChangedArgs);
                return;
            case nameof(SelectedChannel):
                if (_styleSelectedChannel.ClearLocal()) NotifyPropertyChanged(SelectedChannelChangedArgs);
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
        if (_styleSelectedChannel.BeginStyle(0)) NotifyPropertyChanged(SelectedChannelChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSelectedColor.CommitStyle())
            NotifyPropertyChanged(SelectedColorChangedArgs);
        if (_styleColorModel.CommitStyle())
            NotifyPropertyChanged(ColorModelChangedArgs);
        if (_styleSelectedChannel.CommitStyle()) NotifyPropertyChanged(SelectedChannelChangedArgs);
    }
}
