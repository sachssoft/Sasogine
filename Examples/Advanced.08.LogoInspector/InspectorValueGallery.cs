using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Shows the explicitly registered value catalog without creating every editor at once.</summary>
internal sealed class InspectorValueGallery : ContentControl
{
    private readonly IReadOnlyList<IInspectorValueExample> _examples = InspectorValueCatalog.Create();
    private readonly ContentControl _host = new();
    internal ComboBox Selector { get; } = new()
    {
        Height = 34
    };
    private bool _showPropertyMenus = true;
    internal bool ShowPropertyMenus
    {
        get => _showPropertyMenus;
        set { _showPropertyMenus = value; if (_host.Content is InspectorView inspector) inspector.ShowPropertyMenus = value; }
    }
    internal Control? CurrentEditor { get; private set; }
    internal IReadOnlyList<IInspectorValueExample> Examples => _examples;

    internal InspectorValueGallery()
    {
        ContentHorizontalAlignment = ControlAlignment.Stretch;
        ContentVerticalAlignment = ControlAlignment.Stretch;
        var page = new StackPanel
        {
            Padding = new Insets2(12),
            Spacing = 12
        };
        page.Add(new TextBlock { Text = "REGISTERED VALUE EDITORS" });
        page.Add(new TextBlock { Text = "Choose a value type. Commit precise numeric input with Enter.\nBounds and boxes keep their Position/Size or Min/Max rows together.", WrapText = true });
        foreach (var example in _examples)
            Selector.Items.Add(new SelectorItem(example.Name));
        Selector.SelectionChanged += (_, _) => Refresh();
        page.Add(Selector);
        page.Add(_host);
        page.Add(new TextBlock { Text = "Typed editors · automatic enums · NativeAOT", WrapText = true });
        Content = new ScrollViewer
        {
            Content = page,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        Select("Bounds3");
    }

    internal void Select(string name)
    {
        for (int i = 0; i < _examples.Count; i++)
            if (_examples[i].Name == name)
            {
                Selector.SelectedIndex = i;
                return;
            }

        throw new ArgumentException("Unknown catalog entry.", nameof(name));
    }

    private void Refresh()
    {
        if (Selector.SelectedIndex < 0)
            return;
        var inspector = new InspectorView { ShowPropertyMenus = ShowPropertyMenus };
        CurrentEditor = _examples[Selector.SelectedIndex].Build(inspector);
        _host.Content = inspector;
    }
}
