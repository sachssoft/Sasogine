using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Colors;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Edits any supported color model using a two-channel plane, a selected-channel strip, and alpha.</summary>
/// <remarks>Gradient pixels and textures are updated before drawing and reused until their input channels change.</remarks>
public sealed class ColorPlane : Control
{
    /// <summary>Identifies changes to the color representation.</summary>
    private static readonly PropertyChangedEventArgs ColorModelChangedArgs = new(nameof(ColorModel));
    /// <summary>Identifies changes to the selected strip channel.</summary>
    private static readonly PropertyChangedEventArgs SelectedChannelChangedArgs = new(nameof(SelectedChannel));
    /// <summary>Identifies changes to straight RGBA.</summary>
    private static readonly PropertyChangedEventArgs SelectedColorChangedArgs = new(nameof(SelectedColor));

    /// <summary>Defines the cached plane resolution.</summary>
    private const int PlaneResolution = 96;
    /// <summary>Defines the one-dimensional gradient resolution.</summary>
    private const int StripResolution = 256;

    /// <summary>Stores premultiplied alpha-strip pixels.</summary>
    private readonly Color[] _alphaPixels = new Color[StripResolution];
    /// <summary>Stores opaque plane pixels.</summary>
    private readonly Color[] _planePixels = new Color[PlaneResolution * PlaneResolution];
    /// <summary>Stores opaque selected-channel pixels.</summary>
    private readonly Color[] _stripPixels = new Color[StripResolution];

    /// <summary>Stores the last notified straight color.</summary>
    private Color _observed = Color.White;

    /// <summary>Stores the model used to generate cached gradients.</summary>
    private ColorPickerModel _cachedModel = (ColorPickerModel)(-1);
    /// <summary>Stores the model represented by the live normalized coordinates.</summary>
    private ColorPickerModel _componentModel = ColorPickerModel.Hsv;

    /// <summary>Stores straight RGBA with local-over-style precedence.</summary>
    private StyleProperty<Color> _styleSelectedColor = new(Color.White);

    /// <summary>Stores the active model with local-over-style precedence.</summary>
    private StyleProperty<ColorPickerModel> _styleColorModel = new(ColorPickerModel.Hsv);

    /// <summary>Stores the selected component with local-over-style precedence.</summary>
    private StyleProperty<int> _styleSelectedChannel = new(0);

    /// <summary>Owns the cached alpha gradient.</summary>
    private Texture2D? _alphaTexture;
    /// <summary>Owns the cached two-dimensional gradient.</summary>
    private Texture2D? _planeTexture;
    /// <summary>Owns the cached selected-component gradient.</summary>
    private Texture2D? _stripTexture;

    /// <summary>Stores the two plane coordinates used by the strip cache.</summary>
    private Vector2 _stripCoordinates = new(float.NaN);

    /// <summary>Preserves normalized coordinates during pointer edits, including achromatic hue.</summary>
    private Vector4 _components = new(0, 0, 1, 1);

    /// <summary>Stores the fixed plane coordinate used by the cache.</summary>
    private float _planeCoordinate = float.NaN;

    /// <summary>Stores the selected channel used by the cache.</summary>
    private int _cachedChannel = -1;
    /// <summary>Identifies the captured plane, component, or alpha gesture.</summary>
    private int _drag;

    /// <summary>Stores the RGB cache key independently of alpha.</summary>
    private uint _alphaColor = uint.MaxValue;

    /// <summary>Occurs once when straight RGBA changes.</summary>
    public event EventHandler? ColorChanged;

    /// <summary>Notifies the owning dialog when model coordinates change, including latent achromatic hue.</summary>
    internal event EventHandler? CoordinatesChanged;

    /// <summary>Creates an HSV plane with a hue strip, preserving standalone control defaults.</summary>
    public ColorPlane()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(ColorPlane);

    /// <summary>Gets or sets the representation used by the plane and selected-channel strip.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The model is unsupported.</exception>
    public ColorPickerModel ColorModel
    {
        get => _styleColorModel.Value;
        set
        {
            if ((uint)value > (uint)ColorPickerModel.Oklab) throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleColorModel.SetLocal(value)) NotifyPropertyChanged(ColorModelChangedArgs);
            if (_componentModel != value) ReadColor(SelectedColor);
        }
    }

    /// <summary>Gets or sets the zero-based channel controlled by the vertical strip; alpha remains separate.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside zero through two.</exception>
    public int SelectedChannel
    {
        get => _styleSelectedChannel.Value;
        set
        {
            ValidateChannel(value);
            if (_styleSelectedChannel.SetLocal(value)) NotifyPropertyChanged(SelectedChannelChangedArgs);
        }
    }

    /// <summary>Gets or sets straight RGBA without discarding an unchanged pointer gesture's latent hue.</summary>
    public Color SelectedColor
    {
        get => ConfigSelectedColor;
        set
        {
            ConfigSelectedColor = value;
            bool changed = _observed != value;
            if (!changed && _componentModel == ColorModel) return;
            ReadColor(value);
            _observed = value;
            InvalidateVisual();
            if (changed) ColorChanged?.Invoke(this, EventArgs.Empty);
            CoordinatesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gets or sets straight RGBA for public assignments and the style pipeline.</summary>
    internal Color ConfigSelectedColor
    {
        get => _styleSelectedColor.Value;
        set
        {
            if (_styleSelectedColor.SetLocal(value)) NotifyPropertyChanged(SelectedColorChangedArgs);
        }
    }

    /// <summary>Gets the live normalized model coordinates, retaining components lost by an RGBA round trip.</summary>
    internal Vector4 ModelChannels => _components;

    /// <summary>Gets the two-channel area's local bounds.</summary>
    private Bounds2 Plane => new(Padding.Left, Padding.Top, MathF.Max(1, ContentBounds.Width - 30), MathF.Max(1, ContentBounds.Height - 30));
    /// <summary>Gets the selected-channel strip's local bounds.</summary>
    private Bounds2 ComponentStrip => new(Plane.Right + 8, Plane.Y, 16, Plane.Height);
    /// <summary>Gets the alpha strip's local bounds.</summary>
    private Bounds2 Alpha => new(Plane.X, Plane.Bottom + 8, Plane.Width, 16);
    /// <summary>Gets the horizontal plane channel.</summary>
    private int HorizontalChannel => SelectedChannel == 0 ? 1 : 0;
    /// <summary>Gets the vertical plane channel.</summary>
    private int VerticalChannel => SelectedChannel == 2 ? 1 : 2;

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        ValidateChannel(SelectedChannel);
        if ((uint)ColorModel > (uint)ColorPickerModel.Oklab) throw new InvalidOperationException("Unknown color model.");
        if (_componentModel != ColorModel) ReadColor(SelectedColor);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(250, 230);

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (_observed != SelectedColor)
        {
            ReadColor(SelectedColor);
            _observed = SelectedColor;
            ColorChanged?.Invoke(this, EventArgs.Empty);
            CoordinatesChanged?.Invoke(this, EventArgs.Empty);
        }
        UpdateGradients();
    }

    /// <summary>Reads normalized components while retaining hue for an achromatic color in the same model.</summary>
    /// <param name="color">The new straight RGBA color.</param>
    private void ReadColor(Color color)
    {
        Vector4 next = ColorPickerChannels.Read(color, ColorModel);
        if (_componentModel == ColorModel && (ColorModel is ColorPickerModel.Hsv or ColorPickerModel.Hsl) && next.Y <= 0)
            next.X = _components.X;
        _components = next;
        _componentModel = ColorModel;
    }

    /// <summary>Updates only gradients whose defining coordinates changed, then uploads on the UI update thread.</summary>
    private void UpdateGradients()
    {
        GraphicsDevice? device = Workspace?.Registry.Skin?.Texture?.GraphicsDevice;
        bool created = device is not null && (_planeTexture is null || _planeTexture.GraphicsDevice != device);
        if (created)
        {
            ReleaseTextures();
            _planeTexture = new Texture2D(device!, PlaneResolution, PlaneResolution);
            _stripTexture = new Texture2D(device!, 1, StripResolution);
            _alphaTexture = new Texture2D(device!, StripResolution, 1);
        }
        bool changed = _cachedModel != ColorModel || _cachedChannel != SelectedChannel;
        float fixedValue = ColorPickerChannels.Get(_components, SelectedChannel);
        bool hueStrip = SelectedChannel == 0 && (ColorModel is ColorPickerModel.Hsl or ColorPickerModel.Hsv);
        Vector2 coordinates = hueStrip ? Vector2.Zero : new(ColorPickerChannels.Get(_components, HorizontalChannel), ColorPickerChannels.Get(_components, VerticalChannel));
        if (created || changed || fixedValue != _planeCoordinate)
        {
            Vector4 value = _components;
            value.W = 1;
            for (int y = 0; y < PlaneResolution; y++)
                for (int x = 0; x < PlaneResolution; x++)
                {
                    ColorPickerChannels.Set(ref value, HorizontalChannel, (float)x / (PlaneResolution - 1));
                    ColorPickerChannels.Set(ref value, VerticalChannel, 1f - (float)y / (PlaneResolution - 1));
                    _planePixels[y * PlaneResolution + x] = ColorPickerChannels.Write(value, ColorModel);
                }
            _planeTexture?.SetData(_planePixels);
            _planeCoordinate = fixedValue;
        }
        if (created || changed || coordinates != _stripCoordinates)
        {
            Vector4 value = _components;
            value.W = 1;
            if (hueStrip)
            {
                value.Y = 1;
                value.Z = ColorModel == ColorPickerModel.Hsl ? 0.5f : 1f;
            }
            for (int y = 0; y < StripResolution; y++)
            {
                ColorPickerChannels.Set(ref value, SelectedChannel, 1f - (float)y / (StripResolution - 1));
                _stripPixels[y] = ColorPickerChannels.Write(value, ColorModel);
            }
            _stripTexture?.SetData(_stripPixels);
            _stripCoordinates = coordinates;
        }
        uint rgb = SelectedColor.PackedValue & 0x00ffffffu;
        if (created || rgb != _alphaColor)
        {
            Color color = SelectedColor;
            for (int x = 0; x < StripResolution; x++)
                _alphaPixels[x] = Color.FromNonPremultiplied(color.R, color.G, color.B, x);
            _alphaTexture?.SetData(_alphaPixels);
            _alphaColor = rgb;
        }
        _cachedModel = ColorModel;
        _cachedChannel = SelectedChannel;
    }

    /// <summary>Applies a captured pointer position to the plane, component strip, or alpha strip.</summary>
    /// <param name="absolute">The absolute logical pointer position.</param>
    private void SetPoint(Vector2 absolute)
    {
        float x = absolute.X - Bounds.X;
        float y = absolute.Y - Bounds.Y;
        if (_drag == 1)
        {
            ColorPickerChannels.Set(ref _components, HorizontalChannel, Math.Clamp((x - Plane.X) / Plane.Width, 0, 1));
            ColorPickerChannels.Set(ref _components, VerticalChannel, 1 - Math.Clamp((y - Plane.Y) / Plane.Height, 0, 1));
        }
        else if (_drag == 2)
            ColorPickerChannels.Set(ref _components, SelectedChannel, 1 - Math.Clamp((y - ComponentStrip.Y) / ComponentStrip.Height, 0, 1));
        else if (_drag == 3)
            _components.W = Math.Clamp((x - Alpha.X) / Alpha.Width, 0, 1);
        CommitComponents();
    }

    /// <summary>Applies normalized numeric input without collapsing achromatic hue or saturation.</summary>
    /// <param name="value">The three model coordinates and alpha, each between zero and one.</param>
    internal void SetModelChannels(Vector4 value)
    {
        _components = value;
        CommitComponents();
    }

    /// <summary>Commits live coordinates to straight RGBA and updates the owning numeric editors.</summary>
    private void CommitComponents()
    {
        Color next = ColorPickerChannels.Write(_components, ColorModel);
        ConfigSelectedColor = next;
        InvalidateVisual();
        if (next != _observed)
        {
            _observed = next;
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
        CoordinatesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            float x = input.Position.X - Bounds.X;
            float y = input.Position.Y - Bounds.Y;
            _drag = Contains(Plane, x, y) ? 1 : Contains(ComponentStrip, x, y) ? 2 : Contains(Alpha, x, y) ? 3 : 0;
            if (_drag == 0) return;
            context.CapturePointer(this);
            SetPoint(input.Position);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerMove && _drag != 0)
        {
            SetPoint(input.Position);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left && _drag != 0)
        {
            SetPoint(input.Position);
            _drag = 0;
            context.ReleasePointer(this);
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => _drag = 0;

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        ReleaseTextures();
        _cachedModel = (ColorPickerModel)(-1);
        base.OnDetached();
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (_planeTexture is not null) context.DrawTexture(_planeTexture, _planeTexture.Bounds, Plane, Color.White);
        else DrawFallbackPlane(context);
        if (_stripTexture is not null) context.DrawTexture(_stripTexture, _stripTexture.Bounds, ComponentStrip, Color.White);
        else
            for (int i = 0; i < 32; i++)
                context.FillRectangle(new Bounds2(ComponentStrip.X, ComponentStrip.Y + ComponentStrip.Height * i / 32,
                    ComponentStrip.Width, ComponentStrip.Height / 32 + 1), _stripPixels[i * (StripResolution - 1) / 31]);
        if (!context.DrawSkinRegion("color_checker", Alpha)) context.FillRectangle(Alpha, Color.Gray);
        if (_alphaTexture is not null) context.DrawTexture(_alphaTexture, _alphaTexture.Bounds, Alpha, Color.White);
        else
            for (int i = 0; i < 32; i++)
                context.FillRectangle(new Bounds2(Alpha.X + Alpha.Width * i / 32, Alpha.Y,
                    Alpha.Width / 32 + 1, Alpha.Height), _alphaPixels[i * (StripResolution - 1) / 31]);
        float horizontal = ColorPickerChannels.Get(_components, HorizontalChannel);
        float vertical = ColorPickerChannels.Get(_components, VerticalChannel);
        var marker = new Bounds2(Plane.X + Plane.Width * horizontal - 4, Plane.Y + Plane.Height * (1 - vertical) - 4, 8, 8);
        context.StrokeRectangle(marker, Color.Black, 2);
        context.StrokeRectangle(marker, Color.White, 1);
        float selected = ColorPickerChannels.Get(_components, SelectedChannel);
        context.StrokeRectangle(new Bounds2(ComponentStrip.X - 2, ComponentStrip.Y + ComponentStrip.Height * (1 - selected) - 2,
            ComponentStrip.Width + 4, 4), Color.White, 1);
        context.StrokeRectangle(new Bounds2(Alpha.X + Alpha.Width * _components.W - 1, Alpha.Y, 2, Alpha.Height), Color.White, 1);
    }

    /// <summary>Draws a bounded cached fallback when no skin graphics device is available during update.</summary>
    /// <param name="context">The current draw context.</param>
    private void DrawFallbackPlane(ControlDrawContext context)
    {
        const int tiles = 16;
        for (int y = 0; y < tiles; y++)
            for (int x = 0; x < tiles; x++)
                context.FillRectangle(new Bounds2(Plane.X + Plane.Width * x / tiles, Plane.Y + Plane.Height * y / tiles,
                    Plane.Width / tiles + 1, Plane.Height / tiles + 1),
                    _planePixels[y * (PlaneResolution - 1) / (tiles - 1) * PlaneResolution + x * (PlaneResolution - 1) / (tiles - 1)]);
    }

    /// <summary>Releases owned GPU gradients without disposing the workspace's borrowed skin.</summary>
    private void ReleaseTextures()
    {
        _planeTexture?.Dispose(); _planeTexture = null;
        _stripTexture?.Dispose(); _stripTexture = null;
        _alphaTexture?.Dispose(); _alphaTexture = null;
    }

    /// <summary>Rejects alpha or out-of-range indices for the three-dimensional color cube.</summary>
    /// <param name="channel">The selected color channel.</param>
    /// <exception cref="ArgumentOutOfRangeException">The channel is outside zero through two.</exception>
    private static void ValidateChannel(int channel)
    {
        if ((uint)channel > 2) throw new ArgumentOutOfRangeException(nameof(channel));
    }

    /// <summary>Converts normalized HSV coordinates using the engine's color implementation.</summary>
    /// <param name="hue">The normalized hue.</param>
    /// <param name="saturation">The normalized saturation.</param>
    /// <param name="value">The normalized value.</param>
    /// <param name="alpha">The straight alpha.</param>
    /// <returns>The converted color.</returns>
    internal static Color FromHsv(float hue, float saturation, float value, float alpha) => new HsvColor(hue * 360, saturation, value, alpha).ToColor();

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(SelectedColor):
                    if (_styleSelectedColor.Apply(Converters.Convert<Color>("Color", entry.Value))) NotifyPropertyChanged(SelectedColorChangedArgs);
                    break;
                case nameof(ColorModel):
                    if (_styleColorModel.Apply(Converters.Convert<ColorPickerModel>(nameof(ColorPickerModel), entry.Value))) NotifyPropertyChanged(ColorModelChangedArgs);
                    break;
                case nameof(SelectedChannel):
                    int channel = Converters.Convert<int>("Int32", entry.Value);
                    ValidateChannel(channel);
                    if (_styleSelectedChannel.Apply(channel)) NotifyPropertyChanged(SelectedChannelChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(SelectedColor) => _styleSelectedColor.HasLocalValue,
        nameof(ColorModel) => _styleColorModel.HasLocalValue,
        nameof(SelectedChannel) => _styleSelectedChannel.HasLocalValue,
        _ => base.IsStyleLocal(property)
    };

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(SelectedColor):
                if (_styleSelectedColor.ClearLocal()) NotifyPropertyChanged(SelectedColorChangedArgs);
                break;
            case nameof(ColorModel):
                if (_styleColorModel.ClearLocal()) NotifyPropertyChanged(ColorModelChangedArgs);
                break;
            case nameof(SelectedChannel):
                if (_styleSelectedChannel.ClearLocal()) NotifyPropertyChanged(SelectedChannelChangedArgs);
                break;
            default: base.ResetStyleProperty(property); break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleSelectedColor.BeginStyle(Color.White)) NotifyPropertyChanged(SelectedColorChangedArgs);
        if (_styleColorModel.BeginStyle(ColorPickerModel.Hsv)) NotifyPropertyChanged(ColorModelChangedArgs);
        if (_styleSelectedChannel.BeginStyle(0)) NotifyPropertyChanged(SelectedChannelChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSelectedColor.CommitStyle()) NotifyPropertyChanged(SelectedColorChangedArgs);
        if (_styleColorModel.CommitStyle()) NotifyPropertyChanged(ColorModelChangedArgs);
        if (_styleSelectedChannel.CommitStyle()) NotifyPropertyChanged(SelectedChannelChangedArgs);
    }
}
