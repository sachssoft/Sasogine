using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;
using Myra.Graphics2D;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Exercises the unmodified, pinned Myra NuGet package through public APIs.</summary>
internal sealed class MyraTree : IBenchmarkTree
{
    private readonly TreeSpec _spec;
    private readonly Desktop _desktop;
    private readonly Myra.Graphics2D.UI.Button[] _buttons;
    /// <summary>Builds the equivalent Myra tree with the prepared shared stylesheet.</summary>
    public MyraTree(TreeSpec spec, SharedFont font, Stylesheet styles)
    {
        _spec = spec;
        var page = new VerticalStackPanel(styles)
        {
            Spacing = 0
        };
        _buttons = new Myra.Graphics2D.UI.Button[spec.Count];
        for (int row = 0; row < spec.Rows; row++)
        {
            var line = new HorizontalStackPanel(styles)
            {
                Height = spec.CellHeight,
                Spacing = 2
            };
            for (int column = 0; column < spec.Columns; column++)
            {
                var text = new Myra.Graphics2D.UI.Label(styles)
                {
                    Text = "UI",
                    Font = font.Font,
                    TextColor = Color.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var button = new Myra.Graphics2D.UI.Button(styles)
                {
                    Content = text,
                    Width = spec.CellWidth - 2,
                    Height = spec.CellHeight - 2,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                line.Widgets.Add(button);
                _buttons[row * spec.Columns + column] = button;
            }

            page.Widgets.Add(line);
        }

        _desktop = new Desktop
        {
            Root = page,
            BoundsFetcher = static () => new Rectangle(0, 0, 1200, 800)
        };
    }

    /// <summary>Gets the library name used in result rows.</summary>
    public string Library => "Myra";

    /// <summary>Prepares layout outside the measured interval.</summary>
    public void Prepare() => _desktop.UpdateLayout();
    /// <summary>Renders the already arranged tree.</summary>
    public void Draw() => _desktop.Render();
    /// <summary>Leaves update to Desktop.Render, which bundles input, layout, and drawing.</summary>
    public void Update()
    {
    }

    /// <summary>Checks that every button occupies the expected visible cell.</summary>
    public void Verify()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            var b = _buttons[i].ToGlobal(Point.Zero);
            if (b.X != (i % _spec.Columns) * _spec.CellWidth || b.Y != (i / _spec.Columns) * _spec.CellHeight || _buttons[i].Bounds.Width != _spec.CellWidth - 2 || _buttons[i].Bounds.Height != _spec.CellHeight - 2)
                throw new InvalidOperationException($"Myra geometry differs at button {i}: {b}, {_buttons[i].Bounds}");
        }

        if (_desktop.CalculateTotalWidgets(false) != 1 + _spec.Rows + 2 * _spec.Count)
            throw new InvalidOperationException("Myra widget count differs.");
    }

    /// <summary>Detaches the tree and releases its owned resources.</summary>
    public void Dispose() => _desktop.Dispose();
}
