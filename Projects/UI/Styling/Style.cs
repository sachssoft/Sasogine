using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>
/// A named style containing string property names and string values.
/// </summary>
public sealed class Style
{
    /// <summary>Gets or sets the text color while the link is hovered or keyboard-focused.</summary>
    public Color HoverTextColor
    {
        get => _values.TryGetValue(nameof(HoverTextColor), out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(126, 200, 255);
        set => this[nameof(HoverTextColor)] = ShorthandConverters.Format("Color", value);
    }

    /// <summary>Gets or sets the text color while activation is held.</summary>
    public Color PressedTextColor
    {
        get => _values.TryGetValue(nameof(PressedTextColor), out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(60, 130, 210);
        set => this[nameof(PressedTextColor)] = ShorthandConverters.Format("Color", value);
    }

    /// <summary>Gets or sets whether generated text content is underlined. Defaults to true.</summary>
    public bool Underline
    {
        get => _values.TryGetValue(nameof(Underline), out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this[nameof(Underline)] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets the text color after activation when the link is neither hovered nor held.</summary>
    public Color VisitedTextColor
    {
        get => _values.TryGetValue(nameof(VisitedTextColor), out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(178, 147, 232);
        set => this[nameof(VisitedTextColor)] = ShorthandConverters.Format("Color", value);
    }

    /// <summary>Gets or sets the minimum logical tile dimensions. Both components must be finite and positive. Defaults to 16 by 16.</summary>
    public Size2 MinTileSize
    {
        get => _values.TryGetValue(nameof(MinTileSize), out string? text) ? ShorthandConverters.Convert<Size2>("Size2", text) : new Size2(16);
        set => this[nameof(MinTileSize)] = ShorthandConverters.Format("Size2", value);
    }

    /// <summary>Gets or sets the preferred logical tile dimensions and, when enabled, their aspect ratio. Defaults to 128 by 128.</summary>
    public Size2 PreferredTileSize
    {
        get => _values.TryGetValue(nameof(PreferredTileSize), out string? text) ? ShorthandConverters.Convert<Size2>("Size2", text) : new Size2(128);
        set => this[nameof(PreferredTileSize)] = ShorthandConverters.Format("Size2", value);
    }

    /// <summary>Gets or sets whether resizing preserves the preferred aspect ratio. Defaults to true; both minimum dimensions still apply.</summary>
    public bool PreserveAspectRatio
    {
        get => _values.TryGetValue(nameof(PreserveAspectRatio), out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this[nameof(PreserveAspectRatio)] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets a status strip's fixed, automatic or proportional slot length. Defaults to Auto.</summary>
    public GridLength Length
    {
        get => _values.TryGetValue(nameof(Length), out string? text) ? ShorthandConverters.Convert<GridLength>("GridLength", text) : GridLength.Auto;
        set => this[nameof(Length)] = ShorthandConverters.Format("GridLength", value);
    }

    /// <summary>Gets or sets the minimum logical inspector row width for inline labels. Defaults to 420.</summary>
    public float LabelInlineMinWidth
    {
        get => _values.TryGetValue(nameof(LabelInlineMinWidth), out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 420;
        set => this[nameof(LabelInlineMinWidth)] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets whether touch swiping and optional mouse dragging can scroll the viewport. Defaults to true.</summary>
    public bool IsSwipeEnabled
    {
        get => _values.TryGetValue("IsSwipeEnabled", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsSwipeEnabled"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets whether the primary mouse button can drag-scroll content when IsSwipeEnabled is true. Defaults to false.</summary>
    public bool IsMouseDragEnabled
    {
        get => _values.TryGetValue("IsMouseDragEnabled", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsMouseDragEnabled"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets whether IconValueDisplay draws its shared background, border and focus decoration. Defaults to true.</summary>
    public bool UseStyle
    {
        get => _values.TryGetValue("UseStyle", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["UseStyle"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets the enabled navigation inputs. Defaults to All; flags can be combined.</summary>
    public CarouselNavigationMode NavigationMode
    {
        get => _values.TryGetValue("NavigationMode", out string? text) ? ShorthandConverters.Convert<CarouselNavigationMode>("CarouselNavigationMode", text) : CarouselNavigationMode.All;
        set => this["NavigationMode"] = ShorthandConverters.Format("CarouselNavigationMode", value);
    }

    /// <summary>Gets or sets whether clickable page indicators are shown. Defaults to true.</summary>
    public bool ShowIndicators
    {
        get => _values.TryGetValue("ShowIndicators", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["ShowIndicators"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets whether Next and Previous wrap at the collection boundaries. Defaults to false.</summary>
    public bool IsLooping
    {
        get => _values.TryGetValue("IsLooping", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsLooping"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets the page transition effect. Defaults to Slide.</summary>
    public CarouselTransition Transition
    {
        get => _values.TryGetValue("Transition", out string? text) ? ShorthandConverters.Convert<CarouselTransition>("CarouselTransition", text) : CarouselTransition.Slide;
        set => this["Transition"] = ShorthandConverters.Format("CarouselTransition", value);
    }

    /// <summary>Gets or sets the finite transition duration in seconds. Zero switches immediately. Defaults to 0.25.</summary>
    public float TransitionDuration
    {
        get => _values.TryGetValue("TransitionDuration", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.25f;
        set => this["TransitionDuration"] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets the positive movement threshold in logical UI units before a swipe takes capture. Defaults to 24.</summary>
    public float SwipeThreshold
    {
        get => _values.TryGetValue("SwipeThreshold", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 24;
        set => this["SwipeThreshold"] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets the positive logical size of navigation buttons. Defaults to 36.</summary>
    public float NavigationButtonSize
    {
        get => _values.TryGetValue("NavigationButtonSize", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 36;
        set => this["NavigationButtonSize"] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets the positive logical size of each page indicator. Defaults to 16.</summary>
    public float IndicatorSize
    {
        get => _values.TryGetValue("IndicatorSize", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 16;
        set => this["IndicatorSize"] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets the nonnegative logical gap between page indicators. Defaults to 6.</summary>
    public float IndicatorSpacing
    {
        get => _values.TryGetValue("IndicatorSpacing", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 6;
        set => this["IndicatorSpacing"] = ShorthandConverters.Format("Single", value);
    }

    /// <summary>Gets or sets the finite automatic page interval in seconds. Zero disables automatic advancement. Defaults to zero.</summary>
    public float AutoPlayInterval
    {
        get => _values.TryGetValue("AutoPlayInterval", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0;
        set => this["AutoPlayInterval"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>Gets or sets the icon edge relative to an IconValueDisplay value. Defaults to Left.</summary>
    public IconPlacement IconPlacement
    {
        get => _values.TryGetValue("IconPlacement", out string? text) ? ShorthandConverters.Convert<IconPlacement>("IconPlacement", text) : Sachssoft.Engine.Controls.IconPlacement.Left;
        set => this["IconPlacement"] = ShorthandConverters.Format("IconPlacement", value);
    }

    /// <summary>Gets or sets the IFormattable format for IconValueDisplay data. Empty uses the object's text representation.</summary>
    public string ValueFormat
    {
        get => _values.TryGetValue("ValueFormat", out string? text) ? text : string.Empty;
        set => this["ValueFormat"] = value;
    }

    /// <summary>Gets or sets the dimensions fitted to content. Manual preserves configured dimensions; automatic axes ignore Width or Height without changing their stored values.</summary>
    public SizeToContent SizeToContent
    {
        get => _values.TryGetValue("SizeToContent", out string? text) ? ShorthandConverters.Convert<SizeToContent>("SizeToContent", text) : Sachssoft.Engine.Controls.SizeToContent.Manual;
        set => this["SizeToContent"] = ShorthandConverters.Format("SizeToContent", value);
    }
    /// <summary>Gets or sets the title-bar edge for the close button. The default is Right.</summary>
    public CloseButtonPlacement CloseButtonPlacement
    {
        get => _values.TryGetValue("CloseButtonPlacement", out string? text) ? ShorthandConverters.Convert<CloseButtonPlacement>("CloseButtonPlacement", text) : Sachssoft.Engine.Controls.CloseButtonPlacement.Right;
        set => this["CloseButtonPlacement"] = ShorthandConverters.Format("CloseButtonPlacement", value);
    }
    /// <summary>Gets or sets whether to display the close button. CanClose must also be true; hiding the button does not block programmatic closing.</summary>
    public bool ShowCloseButton
    {
        get => _values.TryGetValue("ShowCloseButton", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["ShowCloseButton"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>Gets or sets whether the visible close button accepts pointer input. False keeps a disabled button visible and does not block programmatic closing.</summary>
    public bool IsCloseButtonEnabled
    {
        get => _values.TryGetValue("IsCloseButtonEnabled", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsCloseButtonEnabled"] = ShorthandConverters.Format("Boolean", value);
    }

    /// <summary>Gets or sets the header-strip edge; the default is Top.</summary>
    public TabStripPlacement TabStripPlacement
    {
        get => _values.TryGetValue("TabStripPlacement", out string? text) ? ShorthandConverters.Convert<TabStripPlacement>("TabStripPlacement", text) : Sachssoft.Engine.Controls.TabStripPlacement.Top;
        set => this["TabStripPlacement"] = ShorthandConverters.Format("TabStripPlacement", value);
    }

    /// <summary>Gets or sets the logical width of a left or right tab strip, excluding its scrollbar. Defaults to 160.</summary>
    public float HeaderWidth
    {
        get => _values.TryGetValue("HeaderWidth", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 160f;
        set => this["HeaderWidth"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Gets or sets the requested family; null inherits from the parent.
    /// </summary>
    public  string? FontName
    {
        get => _values.TryGetValue("FontName", out var value) ? ShorthandConverters.Convert<string?>("NullableFontName", value) : null;
        set => this["FontName"] = ShorthandConverters.Format("NullableFontName", value);
    }
    /// <summary>
    /// Gets or sets the logical font size; null inherits from the parent.
    /// </summary>
    public  int? FontSize
    {
        get => _values.TryGetValue("FontSize", out var value) ? ShorthandConverters.Convert<int?>("NullableFontSize", value) : null;
        set => this["FontSize"] = ShorthandConverters.Format("NullableFontSize", value);
    }
    /// <summary>
    /// Gets or sets the requested weight; null inherits from the parent.
    /// </summary>
    public  FontWeight? FontWeight
    {
        get => _values.TryGetValue("FontWeight", out var value) ? ShorthandConverters.Convert<FontWeight?>("NullableFontWeight", value) : null;
        set => this["FontWeight"] = ShorthandConverters.Format("NullableFontWeight", value);
    }
    /// <summary>
    /// Gets or sets the requested face style; null inherits from the parent.
    /// </summary>
    public  FontStyle? FontStyle
    {
        get => _values.TryGetValue("FontStyle", out var value) ? ShorthandConverters.Convert<FontStyle?>("NullableFontStyle", value) : null;
        set => this["FontStyle"] = ShorthandConverters.Format("NullableFontStyle", value);
    }

    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
    private readonly ReadOnlyDictionary<string, string> _view;
    private string? _basedOn;
    private int _version;
    internal static readonly ValueConverterRegistry ShorthandConverters = new();
    /// <summary>
    /// Creates a style; a registry assigns an empty ID during registration.
    /// </summary>
    public Style(string id = "", string? basedOn = null)
    {
        Id = id;
        _basedOn = basedOn;
        _view = new(_values);
    }

    /// <summary>
    /// Gets the stable registered ID.
    /// </summary>
    public string Id { get; internal set; }

    /// <summary>
    /// Gets or sets the optional base style ID.
    /// </summary>
    public string? BasedOn
    {
        get => _basedOn;
        set
        {
            if (_basedOn == value)
                return;
            _basedOn = value;
            Changed();
        }
    }

    /// <summary>
    /// Gets explicit string values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values => _view;
    internal int Version => _version;

    private void Changed()
    {
        unchecked
        {
            _version++;
        }
    }

    /// <summary>
    /// Reads or assigns a string value. Names are case-sensitive.
    /// </summary>
    public string this[string property]
    {
        get => _values[property];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(property);
            ArgumentNullException.ThrowIfNull(value);
            if (_values.TryGetValue(property, out var old) && old == value)
                return;
            _values[property] = value;
            Changed();
        }
    }

    /// <summary>
    /// Removes a style value.
    /// </summary>
    public bool Remove(string property)
    {
        if (!_values.Remove(property))
            return false;
        Changed();
        return true;
    }

    /// <summary>
    /// Typed shorthand for the string value IsVisible.
    /// </summary>
    public  bool IsVisible
    {
        get => _values.TryGetValue("IsVisible", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsVisible"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsEnabled.
    /// </summary>
    public  bool IsEnabled
    {
        get => _values.TryGetValue("IsEnabled", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsEnabled"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsHitTestVisible.
    /// </summary>
    public  bool IsHitTestVisible
    {
        get => _values.TryGetValue("IsHitTestVisible", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsHitTestVisible"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsFocusable.
    /// </summary>
    public  bool IsFocusable
    {
        get => _values.TryGetValue("IsFocusable", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsFocusable"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ZIndex.
    /// </summary>
    public  int ZIndex
    {
        get => _values.TryGetValue("ZIndex", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : 0;
        set => this["ZIndex"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TabIndex.
    /// </summary>
    public  int TabIndex
    {
        get => _values.TryGetValue("TabIndex", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : 0;
        set => this["TabIndex"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Width.
    /// </summary>
    public  float? Width
    {
        get => _values.TryGetValue("Width", out string? text) ? ShorthandConverters.Convert<float?>("NullableSingle", text) : null;
        set => this["Width"] = ShorthandConverters.Format("NullableSingle", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Height.
    /// </summary>
    public  float? Height
    {
        get => _values.TryGetValue("Height", out string? text) ? ShorthandConverters.Convert<float?>("NullableSingle", text) : null;
        set => this["Height"] = ShorthandConverters.Format("NullableSingle", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Margin.
    /// </summary>
    public  Insets2 Margin
    {
        get => _values.TryGetValue("Margin", out string? text) ? ShorthandConverters.Convert<Insets2>("Insets2", text) : Insets2.Zero;
        set => this["Margin"] = ShorthandConverters.Format("Insets2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Padding.
    /// </summary>
    public  Insets2 Padding
    {
        get => _values.TryGetValue("Padding", out string? text) ? ShorthandConverters.Convert<Insets2>("Insets2", text) : Insets2.Zero;
        set => this["Padding"] = ShorthandConverters.Format("Insets2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value HorizontalAlignment.
    /// </summary>
    public  ControlAlignment HorizontalAlignment
    {
        get => _values.TryGetValue("HorizontalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Stretch;
        set => this["HorizontalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for the string value VerticalAlignment.
    /// </summary>
    public  ControlAlignment VerticalAlignment
    {
        get => _values.TryGetValue("VerticalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Stretch;
        set => this["VerticalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Opacity.
    /// </summary>
    public  float Opacity
    {
        get => _values.TryGetValue("Opacity", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 1f;
        set => this["Opacity"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Background.
    /// </summary>
    public  Color Background
    {
        get => _values.TryGetValue("Background", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["Background"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value BorderColor.
    /// </summary>
    public  Color BorderColor
    {
        get => _values.TryGetValue("BorderColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["BorderColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value BorderThickness.
    /// </summary>
    public  float BorderThickness
    {
        get => _values.TryGetValue("BorderThickness", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["BorderThickness"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Text.
    /// </summary>
    public  string Text
    {
        get => _values.TryGetValue("Text", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["Text"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Font.
    /// </summary>
    public  IControlFont? Font
    {
        get => _values.TryGetValue("Font", out string? text) ? ShorthandConverters.Convert<IControlFont?>("Font", text) : null;
        set => this["Font"] = ShorthandConverters.Format("Font", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TextColor.
    /// </summary>
    public  Color TextColor
    {
        get => _values.TryGetValue("TextColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.White;
        set => this["TextColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TextHorizontalAlignment.
    /// </summary>
    public  ControlAlignment TextHorizontalAlignment
    {
        get => _values.TryGetValue("TextHorizontalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Start;
        set => this["TextHorizontalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TextVerticalAlignment.
    /// </summary>
    public  ControlAlignment TextVerticalAlignment
    {
        get => _values.TryGetValue("TextVerticalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Center;
        set => this["TextVerticalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for the string value HoverBackground.
    /// </summary>
    public  Color HoverBackground
    {
        get => _values.TryGetValue("HoverBackground", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["HoverBackground"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value PressedBackground.
    /// </summary>
    public  Color PressedBackground
    {
        get => _values.TryGetValue("PressedBackground", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["PressedBackground"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value DisabledBackground.
    /// </summary>
    public  Color DisabledBackground
    {
        get => _values.TryGetValue("DisabledBackground", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["DisabledBackground"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value FocusColor.
    /// </summary>
    public  Color FocusColor
    {
        get => _values.TryGetValue("FocusColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.Transparent;
        set => this["FocusColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Orientation.
    /// </summary>
    public  StackOrientation Orientation
    {
        get => _values.TryGetValue("Orientation", out string? text) ? ShorthandConverters.Convert<StackOrientation>("StackOrientation", text) : StackOrientation.Vertical;
        set => this["Orientation"] = ShorthandConverters.Format("StackOrientation", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Spacing.
    /// </summary>
    public  float Spacing
    {
        get => _values.TryGetValue("Spacing", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["Spacing"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MinWidth.
    /// </summary>
    public  float MinWidth
    {
        get => _values.TryGetValue("MinWidth", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["MinWidth"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MinHeight.
    /// </summary>
    public  float MinHeight
    {
        get => _values.TryGetValue("MinHeight", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["MinHeight"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsChecked.
    /// </summary>
    public  bool? IsChecked
    {
        get => _values.TryGetValue("IsChecked", out string? text) ? ShorthandConverters.Convert<bool?>("NullableBoolean", text) : false;
        set => this["IsChecked"] = ShorthandConverters.Format("NullableBoolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsThreeState.
    /// </summary>
    public  bool IsThreeState
    {
        get => _values.TryGetValue("IsThreeState", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsThreeState"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value GroupName.
    /// </summary>
    public  string GroupName
    {
        get => _values.TryGetValue("GroupName", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["GroupName"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for the string value GlyphSize.
    /// </summary>
    public  Size2 GlyphSize
    {
        get => _values.TryGetValue("GlyphSize", out string? text) ? ShorthandConverters.Convert<Size2>("Size2", text) : new Size2(24);
        set => this["GlyphSize"] = ShorthandConverters.Format("Size2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TextSpacing.
    /// </summary>
    public  float TextSpacing
    {
        get => _values.TryGetValue("TextSpacing", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 8f;
        set => this["TextSpacing"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Axis.
    /// </summary>
    public  ControlOrientation Axis
    {
        get => _values.TryGetValue("Axis", out string? text) ? ShorthandConverters.Convert<ControlOrientation>("ControlOrientation", text) : ControlOrientation.Horizontal;
        set => this["Axis"] = ShorthandConverters.Format("ControlOrientation", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Minimum.
    /// </summary>
    public  float Minimum
    {
        get => _values.TryGetValue("Minimum", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["Minimum"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Maximum.
    /// </summary>
    public  float Maximum
    {
        get => _values.TryGetValue("Maximum", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 100f;
        set => this["Maximum"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Value.
    /// </summary>
    public  float Value
    {
        get => _values.TryGetValue("Value", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["Value"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value SmallChange.
    /// </summary>
    public  float SmallChange
    {
        get => _values.TryGetValue("SmallChange", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 1f;
        set => this["SmallChange"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value LargeChange.
    /// </summary>
    public  float LargeChange
    {
        get => _values.TryGetValue("LargeChange", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 10f;
        set => this["LargeChange"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ViewportSize.
    /// </summary>
    public  float ViewportSize
    {
        get => _values.TryGetValue("ViewportSize", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["ViewportSize"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Step.
    /// </summary>
    public  float Step
    {
        get => _values.TryGetValue("Step", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["Step"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TrackThickness.
    /// </summary>
    public  float TrackThickness
    {
        get => _values.TryGetValue("TrackThickness", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 8f;
        set => this["TrackThickness"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ThumbSize.
    /// </summary>
    public  Size2 ThumbSize
    {
        get => _values.TryGetValue("ThumbSize", out string? text) ? ShorthandConverters.Convert<Size2>("Size2", text) : new Size2(24);
        set => this["ThumbSize"] = ShorthandConverters.Format("Size2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MinimumThumbExtent.
    /// </summary>
    public  float MinimumThumbExtent
    {
        get => _values.TryGetValue("MinimumThumbExtent", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 24f;
        set => this["MinimumThumbExtent"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ScrollBarThickness.
    /// </summary>
    public  float ScrollBarThickness
    {
        get => _values.TryGetValue("ScrollBarThickness", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 20f;
        set => this["ScrollBarThickness"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value HorizontalScrollBarVisibility.
    /// </summary>
    public  ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => _values.TryGetValue("HorizontalScrollBarVisibility", out string? text) ? ShorthandConverters.Convert<ScrollBarVisibility>("ScrollBarVisibility", text) : ScrollBarVisibility.Auto;
        set => this["HorizontalScrollBarVisibility"] = ShorthandConverters.Format("ScrollBarVisibility", value);
    }
    /// <summary>
    /// Typed shorthand for the string value VerticalScrollBarVisibility.
    /// </summary>
    public  ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => _values.TryGetValue("VerticalScrollBarVisibility", out string? text) ? ShorthandConverters.Convert<ScrollBarVisibility>("ScrollBarVisibility", text) : ScrollBarVisibility.Auto;
        set => this["VerticalScrollBarVisibility"] = ShorthandConverters.Format("ScrollBarVisibility", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsReadOnly.
    /// </summary>
    public  bool IsReadOnly
    {
        get => _values.TryGetValue("IsReadOnly", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsReadOnly"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value AcceptsReturn.
    /// </summary>
    public  bool AcceptsReturn
    {
        get => _values.TryGetValue("AcceptsReturn", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["AcceptsReturn"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value AcceptsTab.
    /// </summary>
    public  bool AcceptsTab
    {
        get => _values.TryGetValue("AcceptsTab", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["AcceptsTab"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value IsPassword.
    /// </summary>
    public  bool IsPassword
    {
        get => _values.TryGetValue("IsPassword", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["IsPassword"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MaxLength.
    /// </summary>
    public  int MaxLength
    {
        get => _values.TryGetValue("MaxLength", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : 32767;
        set => this["MaxLength"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Placeholder.
    /// </summary>
    public  string Placeholder
    {
        get => _values.TryGetValue("Placeholder", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["Placeholder"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for the string value HasError.
    /// </summary>
    public  bool HasError
    {
        get => _values.TryGetValue("HasError", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["HasError"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value DisabledTextColor.
    /// </summary>
    public  Color DisabledTextColor
    {
        get => _values.TryGetValue("DisabledTextColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(97, 113, 133);
        set => this["DisabledTextColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value WrapText.
    /// </summary>
    public  bool WrapText
    {
        get => _values.TryGetValue("WrapText", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["WrapText"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ItemHeight.
    /// </summary>
    public  float ItemHeight
    {
        get => _values.TryGetValue("ItemHeight", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 30f;
        set => this["ItemHeight"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MultiSelect.
    /// </summary>
    public  bool MultiSelect
    {
        get => _values.TryGetValue("MultiSelect", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["MultiSelect"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value SelectedIndex.
    /// </summary>
    public  int SelectedIndex
    {
        get => _values.TryGetValue("SelectedIndex", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : -1;
        set => this["SelectedIndex"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for the string value MaxDropDownHeight.
    /// </summary>
    public  float MaxDropDownHeight
    {
        get => _values.TryGetValue("MaxDropDownHeight", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 240f;
        set => this["MaxDropDownHeight"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value LineSpacing.
    /// </summary>
    public  float LineSpacing
    {
        get => _values.TryGetValue("LineSpacing", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 8f;
        set => this["LineSpacing"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TileSize.
    /// </summary>
    public  Size2 TileSize
    {
        get => _values.TryGetValue("TileSize", out string? text) ? ShorthandConverters.Convert<Size2>("Size2", text) : new Size2(120, 80);
        set => this["TileSize"] = ShorthandConverters.Format("Size2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Columns.
    /// </summary>
    public  int Columns
    {
        get => _values.TryGetValue("Columns", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : 0;
        set => this["Columns"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Title.
    /// </summary>
    public  string Title
    {
        get => _values.TryGetValue("Title", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["Title"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for the string value Position.
    /// </summary>
    public  Vector2? Position
    {
        get => _values.TryGetValue("Position", out string? text) ? ShorthandConverters.Convert<Vector2?>("NullableVector2", text) : null;
        set => this["Position"] = ShorthandConverters.Format("NullableVector2", value);
    }
    /// <summary>
    /// Typed shorthand for the string value CanResize.
    /// </summary>
    public  bool CanResize
    {
        get => _values.TryGetValue("CanResize", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["CanResize"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value CanMove.
    /// </summary>
    public  bool CanMove
    {
        get => _values.TryGetValue("CanMove", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["CanMove"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value CanClose.
    /// </summary>
    public  bool CanClose
    {
        get => _values.TryGetValue("CanClose", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["CanClose"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for the string value TitlebarHeight.
    /// </summary>
    public  float TitlebarHeight
    {
        get => _values.TryGetValue("TitlebarHeight", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 36f;
        set => this["TitlebarHeight"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for the string value SelectedColor.
    /// </summary>
    public  Color SelectedColor
    {
        get => _values.TryGetValue("SelectedColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.White;
        set => this["SelectedColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ContentHorizontalAlignment.
    /// </summary>
    public  ControlAlignment ContentHorizontalAlignment
    {
        get => _values.TryGetValue("ContentHorizontalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Center;
        set => this["ContentHorizontalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for the string value ContentVerticalAlignment.
    /// </summary>
    public  ControlAlignment ContentVerticalAlignment
    {
        get => _values.TryGetValue("ContentVerticalAlignment", out string? text) ? ShorthandConverters.Convert<ControlAlignment>("ControlAlignment", text) : ControlAlignment.Center;
        set => this["ContentVerticalAlignment"] = ShorthandConverters.Format("ControlAlignment", value);
    }
    /// <summary>
    /// Typed shorthand for image Region.
    /// </summary>
    public  string Region
    {
        get => _values.TryGetValue("Region", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["Region"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for image Stretch.
    /// </summary>
    public  ImageStretch Stretch
    {
        get => _values.TryGetValue("Stretch", out string? text) ? ShorthandConverters.Convert<ImageStretch>("ImageStretch", text) : ImageStretch.Uniform;
        set => this["Stretch"] = ShorthandConverters.Format("ImageStretch", value);
    }
    /// <summary>
    /// Typed shorthand for image Tint.
    /// </summary>
    public  Color Tint
    {
        get => _values.TryGetValue("Tint", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : Color.White;
        set => this["Tint"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for Command.
    /// </summary>
    public  System.Windows.Input.ICommand? Command
    {
        get => _values.TryGetValue("Command", out string? text) ? ShorthandConverters.Convert<System.Windows.Input.ICommand?>("Command", text) : null;
        set => this["Command"] = ShorthandConverters.Format("Command", value);
    }
    /// <summary>
    /// Typed shorthand for CommandParameter.
    /// </summary>
    public  object? CommandParameter
    {
        get => _values.TryGetValue("CommandParameter", out string? text) ? ShorthandConverters.Convert<object?>("CommandParameter", text) : null;
        set => this["CommandParameter"] = ShorthandConverters.Format("CommandParameter", value);
    }
    /// <summary>
    /// Typed shorthand for ColorModel.
    /// </summary>
    public  ColorPickerModel ColorModel
    {
        get => _values.TryGetValue("ColorModel", out string? text) ? ShorthandConverters.Convert<ColorPickerModel>("ColorPickerModel", text) : ColorPickerModel.Rgba;
        set => this["ColorModel"] = ShorthandConverters.Format("ColorPickerModel", value);
    }
    /// <summary>
    /// Typed shorthand for HeaderHeight.
    /// </summary>
    public  float HeaderHeight
    {
        get => _values.TryGetValue("HeaderHeight", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 38f;
        set => this["HeaderHeight"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for joystick MovementMode.
    /// </summary>
    public  JoystickMovementMode MovementMode
    {
        get => _values.TryGetValue("MovementMode", out string? text) ? ShorthandConverters.Convert<JoystickMovementMode>("JoystickMovementMode", text) : JoystickMovementMode.Free;
        set => this["MovementMode"] = ShorthandConverters.Format("JoystickMovementMode", value);
    }
    /// <summary>
    /// Typed shorthand for joystick PlacementMode.
    /// </summary>
    public  JoystickPlacementMode PlacementMode
    {
        get => _values.TryGetValue("PlacementMode", out string? text) ? ShorthandConverters.Convert<JoystickPlacementMode>("JoystickPlacementMode", text) : JoystickPlacementMode.Fixed;
        set => this["PlacementMode"] = ShorthandConverters.Format("JoystickPlacementMode", value);
    }
    /// <summary>
    /// Typed shorthand for joystick CustomAxis.
    /// </summary>
    public  Vector2 CustomAxis
    {
        get => _values.TryGetValue("CustomAxis", out string? text) ? ShorthandConverters.Convert<Vector2>("Vector2", text) : Vector2.UnitY;
        set => this["CustomAxis"] = ShorthandConverters.Format("Vector2", value);
    }
    /// <summary>
    /// Typed shorthand for joystick Radius.
    /// </summary>
    public  float Radius
    {
        get => _values.TryGetValue("Radius", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 64f;
        set => this["Radius"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for joystick DeadZone.
    /// </summary>
    public  float DeadZone
    {
        get => _values.TryGetValue("DeadZone", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.12f;
        set => this["DeadZone"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for joystick ReturnToCenter.
    /// </summary>
    public  bool ReturnToCenter
    {
        get => _values.TryGetValue("ReturnToCenter", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["ReturnToCenter"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for joystick InvertX.
    /// </summary>
    public  bool InvertX
    {
        get => _values.TryGetValue("InvertX", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["InvertX"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for joystick InvertY.
    /// </summary>
    public  bool InvertY
    {
        get => _values.TryGetValue("InvertY", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : false;
        set => this["InvertY"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for joystick ResponseCurve.
    /// </summary>
    public  Sachssoft.Engine.Gameplay.Curves.CurveBase? ResponseCurve
    {
        get => _values.TryGetValue("ResponseCurve", out string? text) ? ShorthandConverters.Convert<Sachssoft.Engine.Gameplay.Curves.CurveBase?>("Curve", text) : null;
        set => this["ResponseCurve"] = ShorthandConverters.Format("Curve", value);
    }
    /// <summary>
    /// Typed shorthand for ToolTip.
    /// </summary>
    public  string ToolTip
    {
        get => _values.TryGetValue("ToolTip", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["ToolTip"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for ToolTipContent.
    /// </summary>
    public  Control? ToolTipContent
    {
        get => _values.TryGetValue("ToolTipContent", out string? text) ? ShorthandConverters.Convert<Control?>("Control", text) : null;
        set => this["ToolTipContent"] = ShorthandConverters.Format("Control", value);
    }
    /// <summary>
    /// Typed shorthand for ToolTipDelay.
    /// </summary>
    public  float ToolTipDelay
    {
        get => _values.TryGetValue("ToolTipDelay", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.5f;
        set => this["ToolTipDelay"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for ToolTipDuration.
    /// </summary>
    public  float ToolTipDuration
    {
        get => _values.TryGetValue("ToolTipDuration", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 6f;
        set => this["ToolTipDuration"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for ToolTipMaxWidth.
    /// </summary>
    public  float ToolTipMaxWidth
    {
        get => _values.TryGetValue("ToolTipMaxWidth", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 320f;
        set => this["ToolTipMaxWidth"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for RepeatDelay.
    /// </summary>
    public  float RepeatDelay
    {
        get => _values.TryGetValue("RepeatDelay", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.4f;
        set => this["RepeatDelay"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for RepeatInterval.
    /// </summary>
    public  float RepeatInterval
    {
        get => _values.TryGetValue("RepeatInterval", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.075f;
        set => this["RepeatInterval"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for IsSpinning.
    /// </summary>
    public  bool IsSpinning
    {
        get => _values.TryGetValue("IsSpinning", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["IsSpinning"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for AnimationSpeed.
    /// </summary>
    public  float AnimationSpeed
    {
        get => _values.TryGetValue("AnimationSpeed", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 1f;
        set => this["AnimationSpeed"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for NumericMinimum.
    /// </summary>
    public  decimal NumericMinimum
    {
        get => _values.TryGetValue("NumericMinimum", out string? text) ? ShorthandConverters.Convert<decimal>("Decimal", text) : 0m;
        set => this["NumericMinimum"] = ShorthandConverters.Format("Decimal", value);
    }
    /// <summary>
    /// Typed shorthand for NumericMaximum.
    /// </summary>
    public  decimal NumericMaximum
    {
        get => _values.TryGetValue("NumericMaximum", out string? text) ? ShorthandConverters.Convert<decimal>("Decimal", text) : 100m;
        set => this["NumericMaximum"] = ShorthandConverters.Format("Decimal", value);
    }
    /// <summary>
    /// Typed shorthand for NumericValue.
    /// </summary>
    public  decimal NumericValue
    {
        get => _values.TryGetValue("NumericValue", out string? text) ? ShorthandConverters.Convert<decimal>("Decimal", text) : 0m;
        set => this["NumericValue"] = ShorthandConverters.Format("Decimal", value);
    }
    /// <summary>
    /// Typed shorthand for NumericIncrement.
    /// </summary>
    public  decimal NumericIncrement
    {
        get => _values.TryGetValue("NumericIncrement", out string? text) ? ShorthandConverters.Convert<decimal>("Decimal", text) : 1m;
        set => this["NumericIncrement"] = ShorthandConverters.Format("Decimal", value);
    }
    /// <summary>
    /// Gets or sets the numeric display format. The control supplies its default when omitted.
    /// </summary>
    public string NumberFormat
    {
        get => _values.TryGetValue(nameof(NumberFormat), out string? text) ? text : string.Empty;
        set => this[nameof(NumberFormat)] = value;
    }

    /// <summary>
    /// Typed shorthand for DecimalPlaces.
    /// </summary>
    public  int DecimalPlaces
    {
        get => _values.TryGetValue("DecimalPlaces", out string? text) ? ShorthandConverters.Convert<int>("Int32", text) : 0;
        set => this["DecimalPlaces"] = ShorthandConverters.Format("Int32", value);
    }
    /// <summary>
    /// Typed shorthand for CultureName.
    /// </summary>
    public  string CultureName
    {
        get => _values.TryGetValue("CultureName", out string? text) ? ShorthandConverters.Convert<string>("String", text) : string.Empty;
        set => this["CultureName"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for SelectedDate.
    /// </summary>
    public  DateOnly? SelectedDate
    {
        get => _values.TryGetValue("SelectedDate", out string? text) ? ShorthandConverters.Convert<DateOnly?>("NullableDate", text) : null;
        set => this["SelectedDate"] = ShorthandConverters.Format("NullableDate", value);
    }
    /// <summary>
    /// Typed shorthand for DisplayMonth.
    /// </summary>
    public  DateOnly DisplayMonth
    {
        get => _values.TryGetValue("DisplayMonth", out string? text) ? ShorthandConverters.Convert<DateOnly>("Date", text) : new DateOnly(2000, 1, 1);
        set => this["DisplayMonth"] = ShorthandConverters.Format("Date", value);
    }
    /// <summary>
    /// Typed shorthand for MinimumDate.
    /// </summary>
    public  DateOnly MinimumDate
    {
        get => _values.TryGetValue("MinimumDate", out string? text) ? ShorthandConverters.Convert<DateOnly>("Date", text) : DateOnly.MinValue;
        set => this["MinimumDate"] = ShorthandConverters.Format("Date", value);
    }
    /// <summary>
    /// Typed shorthand for MaximumDate.
    /// </summary>
    public  DateOnly MaximumDate
    {
        get => _values.TryGetValue("MaximumDate", out string? text) ? ShorthandConverters.Convert<DateOnly>("Date", text) : DateOnly.MaxValue;
        set => this["MaximumDate"] = ShorthandConverters.Format("Date", value);
    }
    /// <summary>
    /// Typed shorthand for Today.
    /// </summary>
    public  DateOnly? Today
    {
        get => _values.TryGetValue("Today", out string? text) ? ShorthandConverters.Convert<DateOnly?>("NullableDate", text) : null;
        set => this["Today"] = ShorthandConverters.Format("NullableDate", value);
    }
    /// <summary>
    /// Typed shorthand for FirstDayOfWeek.
    /// </summary>
    public  DayOfWeek FirstDayOfWeek
    {
        get => _values.TryGetValue("FirstDayOfWeek", out string? text) ? ShorthandConverters.Convert<DayOfWeek>("DayOfWeek", text) : DayOfWeek.Monday;
        set => this["FirstDayOfWeek"] = ShorthandConverters.Format("DayOfWeek", value);
    }
    /// <summary>
    /// Typed shorthand for DateFormat.
    /// </summary>
    public  string DateFormat
    {
        get => _values.TryGetValue("DateFormat", out string? text) ? ShorthandConverters.Convert<string>("String", text) : "yyyy-MM-dd";
        set => this["DateFormat"] = ShorthandConverters.Format("String", value);
    }
    /// <summary>
    /// Typed shorthand for Indent.
    /// </summary>
    public  float Indent
    {
        get => _values.TryGetValue("Indent", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 20f;
        set => this["Indent"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for Curve.
    /// </summary>
    public  Sachssoft.Engine.Gameplay.Curves.CurveBase? Curve
    {
        get => _values.TryGetValue("Curve", out string? text) ? ShorthandConverters.Convert<Sachssoft.Engine.Gameplay.Curves.CurveBase?>("Curve", text) : null;
        set => this["Curve"] = ShorthandConverters.Format("Curve", value);
    }
    /// <summary>
    /// Typed shorthand for Interpolation.
    /// </summary>
    public  CurveInterpolation? Interpolation
    {
        get => _values.TryGetValue("Interpolation", out string? text) ? ShorthandConverters.Convert<CurveInterpolation?>("NullableCurveInterpolation", text) : null;
        set => this["Interpolation"] = ShorthandConverters.Format("NullableCurveInterpolation", value);
    }
    /// <summary>
    /// Typed shorthand for ShowGrid.
    /// </summary>
    public  bool ShowGrid
    {
        get => _values.TryGetValue("ShowGrid", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["ShowGrid"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for ShowPoints.
    /// </summary>
    public  bool ShowPoints
    {
        get => _values.TryGetValue("ShowPoints", out string? text) ? ShorthandConverters.Convert<bool>("Boolean", text) : true;
        set => this["ShowPoints"] = ShorthandConverters.Format("Boolean", value);
    }
    /// <summary>
    /// Typed shorthand for SnapX.
    /// </summary>
    public  float SnapX
    {
        get => _values.TryGetValue("SnapX", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.05f;
        set => this["SnapX"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for SnapY.
    /// </summary>
    public  float SnapY
    {
        get => _values.TryGetValue("SnapY", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0.05f;
        set => this["SnapY"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for CurveLineThickness.
    /// </summary>
    public  float CurveLineThickness
    {
        get => _values.TryGetValue("CurveLineThickness", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 2f;
        set => this["CurveLineThickness"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for PointRadius.
    /// </summary>
    public  float PointRadius
    {
        get => _values.TryGetValue("PointRadius", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 5f;
        set => this["PointRadius"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for GridColor.
    /// </summary>
    public  Color GridColor
    {
        get => _values.TryGetValue("GridColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(43, 57, 76);
        set => this["GridColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for CurveColor.
    /// </summary>
    public  Color CurveColor
    {
        get => _values.TryGetValue("CurveColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(65, 199, 223);
        set => this["CurveColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for PointColor.
    /// </summary>
    public  Color PointColor
    {
        get => _values.TryGetValue("PointColor", out string? text) ? ShorthandConverters.Convert<Color>("Color", text) : new Color(227, 234, 244);
        set => this["PointColor"] = ShorthandConverters.Format("Color", value);
    }
    /// <summary>
    /// Typed shorthand for CurveViewMinimum.
    /// </summary>
    public  Vector2 CurveViewMinimum
    {
        get => _values.TryGetValue("CurveViewMinimum", out string? text) ? ShorthandConverters.Convert<Vector2>("Vector2", text) : Vector2.Zero;
        set => this["CurveViewMinimum"] = ShorthandConverters.Format("Vector2", value);
    }
    /// <summary>
    /// Typed shorthand for CurveViewMaximum.
    /// </summary>
    public  Vector2 CurveViewMaximum
    {
        get => _values.TryGetValue("CurveViewMaximum", out string? text) ? ShorthandConverters.Convert<Vector2>("Vector2", text) : Vector2.One;
        set => this["CurveViewMaximum"] = ShorthandConverters.Format("Vector2", value);
    }
    /// <summary>
    /// Typed shorthand for AngleDegrees in degrees.
    /// </summary>
    public  float AngleDegrees
    {
        get => _values.TryGetValue("AngleDegrees", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 0f;
        set => this["AngleDegrees"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for AngleIncrement in degrees.
    /// </summary>
    public  float AngleIncrement
    {
        get => _values.TryGetValue("AngleIncrement", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 1f;
        set => this["AngleIncrement"] = ShorthandConverters.Format("Single", value);
    }
    /// <summary>
    /// Typed shorthand for AngleSnap in degrees.
    /// </summary>
    public  float AngleSnap
    {
        get => _values.TryGetValue("AngleSnap", out string? text) ? ShorthandConverters.Convert<float>("Single", text) : 15f;
        set => this["AngleSnap"] = ShorthandConverters.Format("Single", value);
    }
}
