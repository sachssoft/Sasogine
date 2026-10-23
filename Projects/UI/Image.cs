using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A texture or atlas image suitable for arbitrary control content.
/// </summary>
public class Image : Control
{
    private ChangeTracker<Rectangle?> _rectangle;
    /// <summary>
    /// Creates an image.
    /// </summary>
    public Image() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Image";
    /// <summary>
    /// Gets or sets a borrowed texture.
    /// </summary>
    public Texture2D? Source { get => ConfigSource; set => ConfigSource = value; }
    /// <summary>
    /// Gets or sets a borrowed atlas region.
    /// </summary>
    public string Region { get => ConfigRegion; set => ConfigRegion = value; }
    /// <summary>
    /// Gets or sets image scaling.
    /// </summary>
    public ImageStretch Stretch { get => ConfigStretch; set => ConfigStretch = value; }
    /// <summary>
    /// Gets or sets premultiplied tint.
    /// </summary>
    public Color Tint { get => ConfigTint; set => ConfigTint = value; }

    private (Texture2D? Texture, Rectangle Source) Resolve()
    {
        _rectangle.EnsureChange(ConfigSourceRectangle);
        Texture2D? texture = Source;
        if (texture is null && Region.Length > 0 && Workspace?.Registry.Skin is ControlSkin skin)
            return (skin.Texture, skin[Region].Source);
        Rectangle rectangle = _rectangle.NewValue ?? (texture?.Bounds ?? Rectangle.Empty);
        if (texture is not null && (rectangle.X < 0 || rectangle.Y < 0 || rectangle.Width < 0 || rectangle.Height < 0 || (long)rectangle.Right > texture.Width || (long)rectangle.Bottom > texture.Height))
            throw new InvalidOperationException("Image crop lies outside its texture.");
        return (texture, rectangle);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        var source = Resolve().Source;
        return new Size2(source.Width, source.Height);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        var source = Resolve();
        if (source.Texture is null || source.Source.Width <= 0 || source.Source.Height <= 0)
            return;
        Bounds2 b = new(Padding.Left, Padding.Top, ContentBounds.Width, ContentBounds.Height);
        float w = source.Source.Width, h = source.Source.Height;
        if (Stretch == ImageStretch.Fill)
        {
            w = b.Width;
            h = b.Height;
        }
        else if (Stretch is ImageStretch.Uniform or ImageStretch.UniformToFill)
        {
            float x = b.Width / w, y = b.Height / h, scale = Stretch == ImageStretch.Uniform ? MathF.Min(x, y) : MathF.Max(x, y);
            w *= scale;
            h *= scale;
        }
        else if (Stretch != ImageStretch.None)
            throw new InvalidOperationException("Invalid image stretch mode.");
        using (context.PushClipScope(b))
            context.DrawTexture(source.Texture, source.Source, new Bounds2(b.X + (b.Width - w) / 2, b.Y + (b.Height - h) / 2, w, h), Tint);
    }

    private StyleProperty<string> _styleRegion = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs RegionChangedArgs = new(nameof(Region));
    internal string ConfigRegion
    {
        get => _styleRegion.Value;
        set
        {
            if (_styleRegion.SetLocal(value))
                NotifyPropertyChanged(RegionChangedArgs);
        }
    }

    private StyleProperty<ImageStretch> _styleStretch = new(ImageStretch.Uniform);
    private static readonly System.ComponentModel.PropertyChangedEventArgs StretchChangedArgs = new(nameof(Stretch));
    internal ImageStretch ConfigStretch
    {
        get => _styleStretch.Value;
        set
        {
            if (_styleStretch.SetLocal(value))
                NotifyPropertyChanged(StretchChangedArgs);
        }
    }

    private StyleProperty<Color> _styleTint = new(Color.White);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TintChangedArgs = new(nameof(Tint));
    internal Color ConfigTint
    {
        get => _styleTint.Value;
        set
        {
            if (_styleTint.SetLocal(value))
                NotifyPropertyChanged(TintChangedArgs);
        }
    }

    private Texture2D? _storedSource = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs SourceStorageArgs = new("Source");
    internal  Texture2D? ConfigSource
    {
        get => _storedSource;
        set => SetNotify(ref _storedSource, value, SourceStorageArgs);
    }

    private Rectangle? _storedSourceRectangle = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs SourceRectangleStorageArgs = new("SourceRectangle");
    internal  Rectangle? ConfigSourceRectangle
    {
        get => _storedSourceRectangle;
        set => SetNotify(ref _storedSourceRectangle, value, SourceRectangleStorageArgs);
    }
    /// <summary>
    /// Gets or sets the value for SourceRectangle.
    /// </summary>
    public  Rectangle? SourceRectangle
    {
        get => ConfigSourceRectangle;
        set => ConfigSourceRectangle = value;
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
                case "Region":
                    if (_styleRegion.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(RegionChangedArgs);
                    break;
                case "Stretch":
                    if (_styleStretch.Apply(Converters.Convert<ImageStretch>("ImageStretch", entry.Value)))
                        NotifyPropertyChanged(StretchChangedArgs);
                    break;
                case "Tint":
                    if (_styleTint.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(TintChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Region":
                return _styleRegion.HasLocalValue;
            case "Stretch":
                return _styleStretch.HasLocalValue;
            case "Tint":
                return _styleTint.HasLocalValue;
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
            case "Region":
                if (_styleRegion.ClearLocal())
                    NotifyPropertyChanged(RegionChangedArgs);
                return;
            case "Stretch":
                if (_styleStretch.ClearLocal())
                    NotifyPropertyChanged(StretchChangedArgs);
                return;
            case "Tint":
                if (_styleTint.ClearLocal())
                    NotifyPropertyChanged(TintChangedArgs);
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
        if (_styleRegion.BeginStyle(string.Empty))
            NotifyPropertyChanged(RegionChangedArgs);
        if (_styleStretch.BeginStyle(ImageStretch.Uniform))
            NotifyPropertyChanged(StretchChangedArgs);
        if (_styleTint.BeginStyle(Color.White))
            NotifyPropertyChanged(TintChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleRegion.CommitStyle())
            NotifyPropertyChanged(RegionChangedArgs);
        if (_styleStretch.CommitStyle())
            NotifyPropertyChanged(StretchChangedArgs);
        if (_styleTint.CommitStyle())
            NotifyPropertyChanged(TintChangedArgs);
    }
}
