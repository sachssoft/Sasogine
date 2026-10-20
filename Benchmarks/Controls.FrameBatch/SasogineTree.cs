using Microsoft.Xna.Framework;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Exercises the current Sasogine Controls API without modifying the library.</summary>
internal sealed class SasogineTree : IBenchmarkTree
{
    private readonly ControlInputFrame _frame = new(new Size2(1200, 800), TimeSpan.FromSeconds(1d / 60));
    private readonly TreeSpec _spec;
    private readonly Root _root;
    private readonly Button[] _buttons;
    private readonly ControlLayoutContext _layout;
    private readonly ControlDrawContext _draw;
    /// <summary>Builds a Sasogine tree with direct properties and automatic root registration.</summary>
    public SasogineTree(TreeSpec spec, SharedFont font, ControlDrawContext draw, string library)
    {
        Library = library;
        _spec = spec;
        _draw = draw;
        _layout = new ControlLayoutContext(font);
        var page = new StackPanel
        {
            Padding = Insets2.Zero,
            Margin = Insets2.Zero,
            Spacing = 0,
            Background = Color.Transparent
        };
        _buttons = new Button[spec.Count];
        for (int row = 0; row < spec.Rows; row++)
        {
            var line = new StackPanel
            {
                Orientation = StackOrientation.Horizontal,
                Height = spec.CellHeight,
                Padding = Insets2.Zero,
                Margin = Insets2.Zero,
                Spacing = 2,
                Background = Color.Transparent
            };
            for (int column = 0; column < spec.Columns; column++)
            {
                var text = new TextBlock
                {
                    Text = "UI",
                    TextColor = Color.White,
                    Padding = Insets2.Zero,
                    Margin = Insets2.Zero,
                    Background = Color.Transparent,
                    HorizontalAlignment = ControlAlignment.Center,
                    VerticalAlignment = ControlAlignment.Center
                };
                var button = new Button
                {
                    Content = text,
                    Width = spec.CellWidth - 2,
                    Height = spec.CellHeight - 2,
                    MinWidth = 0,
                    MinHeight = 0,
                    Padding = Insets2.Zero,
                    Margin = Insets2.Zero,
                    BorderThickness = 0,
                    Background = new Color(35, 49, 66),
                    HoverBackground = new Color(35, 49, 66),
                    PressedBackground = new Color(35, 49, 66),
                    FocusColor = Color.Transparent
                };
                line.Add(button);
                _buttons[row * spec.Columns + column] = button;
            }

            page.Add(line);
        }

        _root = new Root(page);
    }

    /// <summary>Gets the library name used in result rows.</summary>
    public string Library { get; }

    /// <summary>Prepares layout outside the measured interval.</summary>
    public void Prepare() => Update();
    /// <summary>Processes one idle UI frame using persistent layout caches.</summary>
    public void Update() => _root.Update(_frame, _layout);
    /// <summary>Renders the already arranged tree.</summary>
    public void Draw() => _root.Draw(_draw);
    /// <summary>Checks that every button occupies the expected visible cell.</summary>
    public void Verify()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            var b = _buttons[i].Bounds;
            if (b.X != (i % _spec.Columns) * _spec.CellWidth || b.Y != (i / _spec.Columns) * _spec.CellHeight || b.Width != _spec.CellWidth - 2 || b.Height != _spec.CellHeight - 2)
                throw new InvalidOperationException($"Sasogine geometry differs at button {i}: {b}");
        }
    }

    /// <summary>Detaches the tree and releases its owned resources.</summary>
    public void Dispose() => _root.Dispose();
}
