using Sachssoft.Engine.Graphics.Text;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Represents uiregistry.
/// </summary>
public sealed class UIRegistry
{
    private readonly Dictionary<string, Func<Control>> _factories = new(StringComparer.Ordinal);
    /// <summary>
    /// Creates the standard styles and all built-in control factories.
    /// </summary>
    /// <param name = "fonts">The borrowed engine registry, or null to create a registry.</param>
    /// <param name = "fontFactory">An optional adapter factory. Each workspace owns and disposes the returned adapters; never return caller-owned fonts.</param>
    public UIRegistry(FontRegistry? fonts = null, Func<FontFace, int, IControlFont>? fontFactory = null)
    {
        Fonts = fonts ?? new FontRegistry();
        FontFactory = fontFactory;
        Styles = CreateStandardStyles();
        Register("Panel", static () => new Panel());
        Register("StackPanel", static () => new StackPanel());
        Register("Label", static () => new Label());
        Register("TextBlock", static () => new TextBlock());
        Register("SceneViewport", static () => new SceneViewport());
        Register("Button", static () => new Button());
        Register("InspectorView", static () => new Inspection.InspectorView());
        RegisterControls();
    }

    /// <summary>
    /// Gets the existing engine registry borrowed by this UI. Register families before updating.
    /// </summary>
    public FontRegistry Fonts { get; }
    internal Func<FontFace, int, IControlFont>? FontFactory { get; }
    /// <summary>
    /// Gets the styles owned by this root.
    /// </summary>
    public StyleRegistry Styles { get; }
    /// <summary>
    /// Gets explicit style converters.
    /// </summary>
    public ValueConverterRegistry Converters { get; } = new();
    /// <summary>
    /// Gets the explicit built-in and application-specific inspector editor factories.
    /// </summary>
    public Inspection.InspectorEditorRegistry InspectorEditors { get; } = new();
    /// <summary>
    /// Gets the exact-type property clipboard shared by inspectors using this registry.
    /// </summary>
    public Inspection.InspectorClipboard InspectorClipboard { get; } = new();
    /// <summary>
    /// Gets the number of control factories.
    /// </summary>
    public int Count => _factories.Count;
    /// <summary>
    /// Gets explicit factory names.
    /// </summary>
    public IEnumerable<string> Kinds => _factories.Keys;

    /// <summary>
    /// Registers a custom control factory. Ordinary new expressions need no registration.
    /// </summary>
    public void Register<T>(string name, Func<T> factory)
        where T : Control
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factories.Add(name, () => factory());
    }

    /// <summary>
    /// Creates an unowned control using a built-in or explicit custom factory.
    /// </summary>
    public Control Create(string name)
    {
        var control = _factories[name]();
        if (control.Workspace is not null || control.Parent is not null || control.FactoryRegistry is not null)
            throw new InvalidOperationException("Factory returned an owned control.");
        control.FactoryRegistry = this;
        return control;
    }

    internal static StyleRegistry CreateStandardStyles()
    {
        var styles = new StyleRegistry();
        styles.Register("Control", new Style());
        styles.Register("Panel", new Style { BasedOn = "Control" });
        styles.Register("StackPanel", new Style { BasedOn = "Panel" });
        styles.Register("Inspector.MultiValue", new Style { BasedOn = "Grid", Spacing = 4, Background = Color.Transparent, BorderThickness = 0 });
        styles.Register("Inspector.PropertyMenu", new Style { BasedOn = "Control", IsFocusable = true, Width = 26, Height = 30, Padding = Insets2.Zero, Background = new Color(32, 35, 42), HoverBackground = new Color(53, 59, 70), PressedBackground = new Color(23, 26, 32), DisabledBackground = new Color(28, 30, 35), TextColor = new Color(230, 233, 239), DisabledTextColor = new Color(115, 121, 132), BorderThickness = 0, VerticalAlignment = ControlAlignment.Start });
        styles.Register("Inspector.Component", new Style { BasedOn = "Grid", Spacing = 2, Background = Color.Transparent, BorderThickness = 0 });
        styles.Register("Label", new Style { BasedOn = "Control" });
        styles.Register("Button", new Style { BasedOn = "ContentControl", IsFocusable = true, Padding = new Insets2(12, 7), Background = new Color(48, 52, 64), ContentHorizontalAlignment = ControlAlignment.Center, HoverBackground = new Color(64, 72, 92), PressedBackground = new Color(36, 84, 132), DisabledBackground = new Color(32, 34, 40), FocusColor = new Color(100, 180, 255) });
        AddStandardStyles(styles);
        return styles;
    }

    /// <summary>
    /// Gets or sets skin.
    /// </summary>
    public ControlSkin? Skin { get; internal set; }
    internal Dictionary<string, SkinStyleState> SkinStyles { get; } = new(StringComparer.Ordinal);
    /// <summary>
    /// Gets or sets clipboard.
    /// </summary>
    public IClipboardService? Clipboard { get; set; }
    /// <summary>
    /// Gets or sets file system.
    /// </summary>
    public IFileDialogProvider FileSystem { get; set; } = new LocalFileDialogProvider();
    internal System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Color>> SkinColors { get; } = new(System.StringComparer.Ordinal);

    private void RegisterControls()
    {
        Register("DropDownButton", static () => new DropDownButton());
        Register("SplitButton", static () => new SplitButton());
        Register("Carousel", static () => new Carousel());
        Register("IconValueDisplay", static () => new IconValueDisplay());
        Register("HeaderContentControl", static () => new HeaderContentControl());
        Register("EdgePanel", static () => new EdgePanel());
        Register("GroupBox", static () => new GroupBox());
        Register("ToolBar", static () => new ToolBar());
        Register("ToolStrip", static () => new ToolStrip());
        Register("ToolBox", static () => new ToolBox());
        Register("StatusBar", static () => new StatusBar());
        Register("StatusStrip", static () => new StatusStrip());
        Register("Expander", static () => new Expander());
        Register(nameof(Header), static () => new Header());
        Register("ValueBar", static () => new ValueBar());
        Register("ValueTextBox", static () => new ValueTextBox());
        Register("Chart", static () => new Chart());
        Register("ScriptTextBox", static () => new ScriptTextBox());
        Register("CurveControl", static () => new CurveControl());
        Register("TreeView", static () => new TreeView());
        Register("Calendar", static () => new Calendar());
        Register("DatePicker", static () => new DatePicker());
        Register("RepeatButton", static () => new RepeatButton());
        Register("NumericSpinner", static () => new NumericSpinner());
        Register("Spinner", static () => new Spinner());
        Register("PasswordBox", static () => new PasswordBox());
        Register("ToolTip", static () => new ToolTip());
        Register("VirtualJoystick", static () => new VirtualJoystick());
        Register("Grid", static () => new Grid());
        Register("TabControl", static () => new TabControl());
        Register("TabItem", static () => new TabItem());
        Register("DataTable", static () => new DataTable());
        Register("ContentControl", static () => new ContentControl());
        Register("Image", static () => new Image());
        Register("ToggleButton", static () => new ToggleButton());
        Register("Switch", static () => new Switch());
        Register("CheckBox", static () => new CheckBox());
        Register("RadioButton", static () => new RadioButton());
        Register("SelectorButton", static () => new SelectorButton());
        Register("RotationControl", static () => new RotationControl());
        Register("Slider", static () => new Slider());
        Register("ScrollBar", static () => new ScrollBar());
        Register("ScrollViewer", static () => new ScrollViewer());
        Register("TextBox", static () => new TextBox());
        Register("ListBox", static () => new ListBox());
        Register("AssetView", static () => new AssetView());
        Register("AssetPickerDialog", static () => new AssetPickerDialog());
        Register("ReferencePicker", static () => new ReferencePicker());
        Register("ComboBox", static () => new ComboBox());
        Register("WrapPanel", static () => new WrapPanel());
        Register("TiledPanel", static () => new TiledPanel());
        Register("MenuBar", static () => new MenuBar());
        Register("ContextMenu", static () => new ContextMenu());
        Register("Window", static () => new Window());
        Register("Dialog", static () => new Dialog());
        Register("OpenFileDialog", static () => new OpenFileDialog());
        Register("FolderSelectorDialog", static () => new FolderSelectorDialog());
        Register("ColorPlane", static () => new ColorPlane());
        Register("ColorPickerDialog", static () => new ColorPickerDialog());
        Register("MessageBox", static () => new MessageBox());
    }

    private static void AddStandardStyles(StyleRegistry styles)
    {
        styles.Register("DropDownButton", new Style { BasedOn = "Button" });
        styles.Register("SplitButton", new Style { BasedOn = "Control" });
        styles.Register("ToolBar", new Style("ToolBar", "WrapPanel") { ["Padding"] = "6", ["Spacing"] = "6", ["LineSpacing"] = "6" });
        styles.Register("ToolStrip", new Style("ToolStrip", "WrapPanel") { ["Padding"] = "4", ["Spacing"] = "4", ["LineSpacing"] = "4", ["BorderThickness"] = "1" });
        styles.Register("ToolBox", new Style("ToolBox", "Panel") { ["Padding"] = "6", ["Spacing"] = "4" });
        styles.Register("StatusBar", new Style("StatusBar", "StackPanel") { ["Padding"] = "4", ["Spacing"] = "8", ["Orientation"] = "Horizontal" });
        styles.Register("StatusStrip", new Style("StatusStrip", "ContentControl") { ["Padding"] = "6,2", ["ContentHorizontalAlignment"] = "Start", ["ContentVerticalAlignment"] = "Center" });
        styles.Register("Carousel", new Style { BasedOn = "Control", IsFocusable = true });
        styles.Register("Carousel.Previous", new Style { BasedOn = "Button", Padding = Insets2.Zero, MinWidth = 0, MinHeight = 0 });
        styles.Register("Carousel.Next", new Style { BasedOn = "Button", Padding = Insets2.Zero, MinWidth = 0, MinHeight = 0 });
        styles.Register("Carousel.Indicator", new Style { BasedOn = "ToggleButton", Padding = Insets2.Zero, MinWidth = 0, MinHeight = 0, BorderThickness = 0, Background = Color.Transparent });
        styles.Register("IconValueDisplay", new Style { BasedOn = "Control", UseStyle = true, Spacing = 8, Padding = new Insets2(10, 6), Background = new Color(28, 35, 48), BorderColor = new Color(56, 71, 92), BorderThickness = 1 });
        styles.Register("HeaderContentControl", new Style { BasedOn = "ContentControl", ContentHorizontalAlignment = ControlAlignment.Stretch, ContentVerticalAlignment = ControlAlignment.Stretch });
        styles.Register("EdgePanel", new Style { BasedOn = "StackPanel", Orientation = StackOrientation.Horizontal });
        styles.Register("GroupBox", new Style { BasedOn = "HeaderContentControl", ContentHorizontalAlignment = ControlAlignment.Stretch, ContentVerticalAlignment = ControlAlignment.Stretch, Background = new Color(28, 35, 48), BorderColor = new Color(56, 71, 92), BorderThickness = 1 });
        styles.Register("Expander", new Style { BasedOn = "GroupBox" });
        styles.Register(nameof(Header), new Style { BasedOn = "GroupBox" });
        styles.Register("ValueTextBox", new Style { BasedOn = "Control" });
        styles.Register("ValueBar", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero, MinHeight = 32, Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92), HoverBackground = new Color(32, 68, 83), PressedBackground = new Color(65, 130, 153), BorderThickness = 1 });
        styles.Register("ValueTextBox.Range", new Style { BasedOn = "ValueBar" });
        styles.Register("ValueTextBox.Editor", new Style { BasedOn = "TextBox", MinWidth = 0, MinHeight = 0, Padding = new Insets2(8, 4) });
        styles.Register("ValueTextBox.Arrow", new Style { BasedOn = "RepeatButton", MinWidth = 0, MinHeight = 0, Padding = Insets2.Zero });
        styles.Register("Chart", new Style { BasedOn = "Control", Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92), Padding = new Insets2(8) });
        styles.Register("ScriptTextBox", new Style { BasedOn = "TextBox" });
        styles.Register("CurveControl", new Style { BasedOn = "Control", IsFocusable = true, Padding = new Insets2(12), Background = new Color(16, 23, 34), BorderColor = new Color(56, 71, 92), BorderThickness = 1, MinHeight = 160, TextColor = new Color(157, 179, 201) });
        styles.Register("TreeView", new Style { BasedOn = "ListBox", Padding = new Insets2(2), IsFocusable = true });
        styles.Register("Calendar", new Style { BasedOn = "Control", Padding = new Insets2(8), IsFocusable = true, Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92), BorderThickness = 1 });
        styles.Register("Calendar.DayText", new Style { BasedOn = "TextBlock" });
        styles.Register("Calendar.OutsideDayText", new Style { BasedOn = "Calendar.DayText", TextColor = new Color(115, 132, 153) });
        styles.Register("Calendar.Day", new Style { BasedOn = "Button", Padding = Insets2.Zero, Background = Color.Transparent });
        styles.Register("Calendar.SelectedDay", new Style { BasedOn = "Calendar.Day", Background = new Color(33, 70, 84), BorderColor = new Color(65, 199, 223), BorderThickness = 1 });
        styles.Register("Calendar.OutsideDay", new Style { BasedOn = "Calendar.Day", TextColor = new Color(115, 132, 153) });
        styles.Register("Calendar.Today", new Style { BasedOn = "Calendar.Day", BorderColor = new Color(65, 199, 223), BorderThickness = 1 });
        styles.Register("DatePicker", new Style { BasedOn = "Control", Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92), BorderThickness = 1, MinHeight = 36 });
        styles.Register("DatePicker.Editor", new Style { BasedOn = "TextBox", Background = Color.Transparent, BorderThickness = 0, MinHeight = 0 });
        styles.Register("RepeatButton", new Style { BasedOn = "Button" });
        styles.Register("NumericSpinner", new Style { BasedOn = "Control", Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92), BorderThickness = 1, MinHeight = 36 });
        styles.Register("NumericSpinner.Editor", new Style { BasedOn = "TextBox", Background = Color.Transparent, BorderThickness = 0, MinHeight = 0 });
        styles.Register("NumericSpinner.Arrow", new Style { BasedOn = "RepeatButton", Padding = Insets2.Zero, MinHeight = 0 });
        styles.Register("Spinner", new Style { BasedOn = "Control", TextColor = new Color(65, 199, 223), IsHitTestVisible = false });
        styles.Register("PasswordBox", new Style { BasedOn = "TextBox" });
        styles.Register("ToolTip", new Style { BasedOn = "ContentControl", Background = new Color(36, 46, 62), BorderColor = new Color(65, 199, 223), BorderThickness = 1, Padding = new Insets2(10), IsHitTestVisible = false });
        styles.Register("VirtualJoystick", new Style { BasedOn = "Control", Padding = new Insets2(8), Radius = 64, ThumbSize = new Size2(40), DeadZone = .12f, Background = Color.Transparent, BorderColor = new Color(56, 71, 92), PressedBackground = new Color(65, 199, 223), TextColor = new Color(157, 179, 201), DisabledBackground = new Color(50, 58, 70) });
        styles.Register("Grid", new Style { BasedOn = "Control", Padding = Insets2.Zero });
        styles.Register("TabControl", new Style { BasedOn = "Panel", Padding = Insets2.Zero });
        styles.Register("TabItem", new Style { BasedOn = "ContentControl", Padding = new Insets2(12), ContentHorizontalAlignment = ControlAlignment.Stretch, ContentVerticalAlignment = ControlAlignment.Stretch });
        styles.Register("TabControl.Header", new Style { BasedOn = "ToggleButton", Height = 38, Padding = new Insets2(14, 6) });
        styles.Register("DataTable", new Style { BasedOn = "ListBox", HeaderHeight = 34, BorderColor = new Color(43, 57, 76), MinHeight = 80 });
        styles.Register("ContentControl", new Style { BasedOn = "Control" });
        styles.Register("InspectorView", new Style { BasedOn = "ContentControl", ContentHorizontalAlignment = ControlAlignment.Stretch, ContentVerticalAlignment = ControlAlignment.Start });
        styles.Register("Image", new Style { BasedOn = "Control" });
        styles["Control"].FocusColor = new Color(65, 199, 223);
        styles.Register("ToggleButton", new Style { BasedOn = "Button" });
        styles.Register("Switch", new Style { BasedOn = "ToggleButton", IsFocusable = true, MinWidth = 52, MinHeight = 44, Padding = Insets2.Zero, ContentHorizontalAlignment = ControlAlignment.Start, Background = new Color(75, 85, 100), PressedBackground = new Color(65, 199, 223), DisabledBackground = new Color(50, 57, 67) });
        styles.Register("CheckBox", new Style { BasedOn = "ToggleButton", IsFocusable = true, Padding = Insets2.Zero, ContentHorizontalAlignment = ControlAlignment.Start });
        styles.Register("RadioButton", new Style { BasedOn = "CheckBox", IsFocusable = true, ContentHorizontalAlignment = ControlAlignment.Start });
        styles.Register("SelectorButton", new Style { BasedOn = "ToggleButton" });
        styles.Register("Slider.Horizontal", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero, Axis = ControlOrientation.Horizontal, MinHeight = 24 });
        styles.Register("Slider.Vertical", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero, Axis = ControlOrientation.Vertical, MinWidth = 24 });
        styles.Register("RotationControl", new Style { BasedOn = "Control", IsFocusable = true, Padding = new Insets2(8), MinWidth = 100, MinHeight = 100, Background = new Color(17, 24, 35), BorderColor = new Color(65, 83, 105), TextColor = new Color(65, 199, 223) });
        styles.Register("TextBlock", new Style { BasedOn = "Label" });
        styles.Register("Slider", new Style { BasedOn = "Slider.Horizontal" });
        styles.Register("ScrollBar.Horizontal", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero, Axis = ControlOrientation.Horizontal });
        styles.Register("ScrollBar.Vertical", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero, Axis = ControlOrientation.Vertical });
        styles.Register("ScrollBar", new Style { BasedOn = "ScrollBar.Vertical" });
        styles.Register("ScrollViewer", new Style { IsSwipeEnabled = true, IsMouseDragEnabled = false, SwipeThreshold = 12, BasedOn = "Panel", Padding = Insets2.Zero });
        styles.Register("TextBox", new Style { BasedOn = "Control", IsFocusable = true, Padding = new Insets2(10, 6), MinHeight = 32, Background = new Color(20, 28, 40), BorderColor = new Color(56, 71, 92) });
        styles.Register("ListBox", new Style { BasedOn = "Control", IsFocusable = true, Padding = new Insets2(2), Background = new Color(20, 28, 40) });
        styles.Register("AssetView", new Style { BasedOn = "ListBox" });
        styles.Register("AssetPickerDialog", new Style { BasedOn = "Dialog", Title = "Choose asset", Width = 720, Height = 520 });
        styles.Register("ReferencePicker", new Style { BasedOn = "ContentControl", MinHeight = 32 });
        styles.Register("ComboBox", new Style { BasedOn = "TextBox" });
        styles.Register("WrapPanel", new Style { BasedOn = "Panel", Padding = Insets2.Zero, Spacing = 8 });
        styles.Register("TiledPanel", new Style { BasedOn = "Panel", Padding = Insets2.Zero, Spacing = 8 });
        styles.Register("MenuBar", new Style { BasedOn = "Control", IsFocusable = true, Height = 36, Padding = Insets2.Zero });
        styles.Register("ContextMenu", new Style { BasedOn = "Control", IsFocusable = true, Padding = new Insets2(6) });
        styles.Register("Window", new Style { BasedOn = "ContentControl", ContentHorizontalAlignment = ControlAlignment.Stretch, ContentVerticalAlignment = ControlAlignment.Stretch, Width = 560, Height = 360, MinWidth = 220, MinHeight = 120, Padding = new Insets2(16), TitlebarHeight = 36 });
        styles.Register("Dialog", new Style { BasedOn = "Window" });
        styles.Register("OpenFileDialog", new Style { BasedOn = "Dialog", Title = "Open file", Width = 640, Height = 440, MinWidth = 360, MinHeight = 260 });
        styles.Register("FolderSelectorDialog", new Style { BasedOn = "Dialog", Title = "Select folder", Width = 640, Height = 440, MinWidth = 360, MinHeight = 260 });
        styles.Register("ColorPlane", new Style { BasedOn = "Control", IsFocusable = true, Padding = Insets2.Zero });
        styles.Register("ColorPickerDialog", new Style { BasedOn = "Dialog", Title = "Choose color", Width = 600, Height = 400, MinWidth = 420, MinHeight = 320 });
        styles.Register("MessageBox", new Style { BasedOn = "Dialog", Width = 440, Height = 240, MinWidth = 280, MinHeight = 160, CanResize = false });
    }
}
