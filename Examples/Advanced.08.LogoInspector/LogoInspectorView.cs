using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Explicit inspector descriptions for one engine entity, nested definitions and tracked accent items.</summary>
internal sealed class LogoInspectorView : IDisposable
{
    internal LogoDefinition Definition { get; } = new();
    internal LogoEntity Entity { get; }
    internal Workspace Workspace { get; }
    internal LogoCanvas Canvas { get; }
    internal InspectorGroup TransformGroup { get; }
    internal InspectorCollection<LogoAccentDefinition> AccentEditor { get; }
    internal MultiValueEditor<Vector2> PositionEditor { get; }
    internal MultiValueEditor<Vector2> ScaleEditor { get; }
    internal MultiValueEditor<Vector3> SpatialPositionEditor { get; }
    internal MultiValueEditor<Vector3> SpatialRotationEditor { get; }
    internal MultiValueEditor<Vector3> SpatialScaleEditor { get; }
    internal TextBox TitleEditor { get; }
    internal TabControl InspectorTabs { get; } = new();
    internal InspectorValueGallery ValueGallery { get; } = new();

    internal InspectorAssetGallery AssetGallery { get; }
    internal CheckBox MenuVisibility { get; } = new() { IsChecked = true, Content = new TextBlock { Text = "Property menus" } };

    private int _accentNumber = 2;
    internal LogoInspectorView()
    {
        AssetGallery = new InspectorAssetGallery(Definition.Appearance);
        Entity = new LogoEntity(Definition);
        Entity.Load();
        Entity.Synchronize();
        Canvas = new LogoCanvas(Entity);
        var page = new Grid
        {
            Spacing = 14,
            Padding = new Insets2(18)
        };
        page.Rows.Add(new RowTrack { Height = new GridLength(52) });
        page.Rows.Add(new RowTrack { Height = GridLength.Star() });
        page.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        page.Tracks.Add(new ColumnTrack { Width = new GridLength(520) });
        var heading = new StackPanel
        {
            Spacing = 2
        };
        heading.Add(new TextBlock { Text = "SASOGINE   /   ENTITY INSPECTOR" });
        heading.Add(new TextBlock { Text = "Live definition editing · expandable groups · tracked collections", TextColor = new Color(139, 157, 184) });
        page.Add(heading, 0, 0, columnSpan: 2);
        page.Add(Canvas, 1, 0);
        var inspector = new InspectorView();
        var showMenus = MenuVisibility;
        showMenus.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CheckBox.IsChecked))
            {
                bool visible = showMenus.IsChecked == true;
                inspector.ShowPropertyMenus = visible;
                ValueGallery.ShowPropertyMenus = visible;
                AssetGallery.Inspector.ShowPropertyMenus = visible;
            }
        };
        heading.Remove(heading.Children[1]);
        heading.Add(showMenus);
        var identity = inspector.AddGroup("Logo entity").Fields;
        TitleEditor = identity.AddText("Title", () => Definition.Title, value => Definition.Title = value, Definition, nameof(Definition.Title));
        identity.AddText("Subtitle", () => Definition.Subtitle, value => Definition.Subtitle = value, Definition, nameof(Definition.Subtitle));
        identity.AddBoolean("Visible", () => Definition.Visible, value => Definition.Visible = value, Definition, nameof(Definition.Visible));
        var definition = inspector.AddGroup("Definition").Fields;
        TransformGroup = definition.AddGroup("Transform · 2D");
        var transform = TransformGroup.Fields;
        var t = Definition.Transform;
        PositionEditor = transform.AddValues("Position", () => t.Position, v => t.Position = v, new[] { new ValueComponent<Vector2>("X", static v => v.X, static (v, x) => new(x, v.Y), -180, 180), new ValueComponent<Vector2>("Y", static v => v.Y, static (v, y) => new(v.X, y), -120, 120) }, t, nameof(t.Position));
        ScaleEditor = transform.AddVector2("Scale", () => t.Scale, v => t.Scale = v, .5f, 1.4f, .01f, t, nameof(t.Scale));
        transform.AddSingle("Rotation", () => t.Rotation, v => t.Rotation = v, -180, 180, 1, t, nameof(t.Rotation));
        transform.SetDefault(PositionEditor, Vector2.Zero);
        transform.SetDefault(ScaleEditor, Vector2.One);
        var spatial = definition.AddGroup("Transform · 3D (projected)").Fields;
        var s = Definition.Spatial;
        SpatialPositionEditor = spatial.AddVector3("Position", () => s.Position, v => s.Position = v, -180, 180, 1, s, nameof(s.Position));
        SpatialRotationEditor = spatial.AddVector3("Rotation", () => s.Rotation, v => s.Rotation = v, -180, 180, 1, s, nameof(s.Rotation));
        SpatialScaleEditor = spatial.AddVector3("Scale", () => s.Scale, v => s.Scale = v, .5f, 1.4f, .01f, s, nameof(s.Scale));
        spatial.SetDefault(SpatialPositionEditor, Vector3.Zero);
        spatial.SetDefault(SpatialRotationEditor, Vector3.Zero);
        spatial.SetDefault(SpatialScaleEditor, Vector3.One);
        var appearance = definition.AddGroup("Appearance", false).Fields;
        var a = Definition.Appearance;
        appearance.AddColor("Primary", () => a.Primary, v => a.Primary = v, a, nameof(a.Primary));
        appearance.AddColor("Secondary", () => a.Secondary, v => a.Secondary = v, a, nameof(a.Secondary));
        appearance.AddSingle("Stroke", () => a.Stroke, v => a.Stroke = v, 5, 22, 1, a, nameof(a.Stroke));
        appearance.AddBoolean("Frame", () => a.ShowFrame, v => a.ShowFrame = v, a, nameof(a.ShowFrame));
        var accents = inspector.AddGroup("Accents · TrackableCollection").Fields;
        AccentEditor = new InspectorCollection<LogoAccentDefinition>(Definition.Accents, () => new LogoAccentDefinition { Name = "Accent " + (++_accentNumber) }, item => item.Name, DescribeAccent);
        accents.Add(AccentEditor);
        InspectorTabs.Items.Add(new TabItem { Header = "Logo", Content = new ScrollViewer { Content = inspector, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }, Padding = Insets2.Zero });
        InspectorTabs.Items.Add(new TabItem { Header = "Value types", Content = ValueGallery, Padding = Insets2.Zero });
        InspectorTabs.Items.Add(new TabItem { Header = "Assets", Content = AssetGallery, Padding = Insets2.Zero });
        InspectorTabs.SelectedIndex = 0;
        page.Add(InspectorTabs, 1, 1);
        Workspace = new Workspace(page);
        Workspace.Styles.Register("Logo.Canvas", new Style { BasedOn = "Control", Background = new Color(12, 18, 29), TextColor = new Color(237, 243, 255) });
    }

    private static void DescribeAccent(InspectorView fields, LogoAccentDefinition item)
    {
        fields.AddText("Name", () => item.Name, v => item.Name = v, item, nameof(item.Name));
        fields.AddSingle("Angle", () => item.Angle, v => item.Angle = v, -180, 180, 1, item, nameof(item.Angle));
        fields.AddSingle("Length", () => item.Length, v => item.Length = v, 8, 75, 1, item, nameof(item.Length));
        fields.AddBoolean("Visible", () => item.Visible, v => item.Visible = v, item, nameof(item.Visible));
    }

    internal void Update()
    {
        Entity.Synchronize();
        Canvas.SynchronizeText();
    }

    public void Dispose()
    {
        Workspace.Dispose();
        AssetGallery.Dispose();
        Entity.Dispose();
        Entity.Unload();
    }
}
