using Sachssoft.Engine.Input;
using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Live frame telemetry, script editing and grouped command controls.</summary>
public sealed class ControlsToolsPage : Grid
{
    private readonly ChartSeries _frameTimes = new(180)
    {
        Name = "Frame time (ms)",
        Color = new Color(65, 199, 223)
    };
    private readonly ChartSeries _fps = new(120)
    {
        Name = "FPS",
        Color = new Color(237, 191, 105)
    };
    private readonly TextBlock _status = new()
    {
        Text = "Primary action: ready"
    };
    private double _sampleTime;
    private int _frames;
    /// <summary>Creates the tools gallery. Its live values use the actual update interval.</summary>
    public ControlsToolsPage()
    {
        Padding = new Insets2(20);
        Spacing = 14;
        Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        Rows.Add(new RowTrack { Height = GridLength.Star() });
        var left = new StackPanel
        {
            Spacing = 10
        };
        var right = new Grid
        {
            Spacing = 10
        };
        right.Rows.Add(new RowTrack { Height = GridLength.Auto });
        right.Rows.Add(new RowTrack { Height = GridLength.Star() });
        right.Rows.Add(new RowTrack { Height = GridLength.Auto });
        Add(left);
        Add(right, column: 1);
        left.Add(new TextBlock { Text = "LIVE / frame time (ms), 0–50", Height = 24 });
        var line = new Chart
        {
            Height = 125,
            AutoScale = false,
            Maximum = 50
        };
        line.Series.Add(_frameTimes);
        left.Add(line);
        left.Add(new TextBlock { Text = "FPS / latest samples, 0–200", Height = 24 });
        var bars = new Chart
        {
            Height = 105,
            Kind = ChartKind.Bar,
            AutoScale = false,
            Maximum = 200
        };
        bars.Series.Add(_fps);
        left.Add(bars);
        var buttons = new StackPanel
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 8
        };
        left.Add(buttons);
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Text = "Clear telemetry", Action = () =>
        {
            _frameTimes.Clear();
            _fps.Clear();
        } });
        menu.Items.Add(new MenuItem { Text = "Line / step", Action = () => line.Kind = line.Kind == ChartKind.Line ? ChartKind.Step : ChartKind.Line });
        buttons.Add(new DropDownButton { Content = new TextBlock { Text = "Chart" }, Menu = menu, Height = 36 });
        var exportMenu = new ContextMenu();
        exportMenu.Items.Add(new MenuItem { Text = "Export options", Action = () => _status.Text = "Choose an export target in your application." });
        buttons.Add(new SplitButton { Content = new TextBlock { Text = "Export" }, Menu = exportMenu, Height = 36, Command = new Command(p => _status.Text = "Primary action: " + p), CommandParameter = "telemetry" });
        left.Add(_status);
        var settings = new StackPanel
        {
            Spacing = 6
        };
        settings.Add(new TextBlock { Text = "Sensitivity / click to type, drag to adjust" });
        settings.Add(new ValueTextBox { Minimum = 0, Maximum = 2, Step = .05f, Value = 1, Height = 36 });
        left.Add(new Expander { HeaderContent = new TextBlock { Text = "Settings / click to collapse" }, Content = settings });
        left.Add(new GroupBox { Header = new TextBlock { Text = "Tip" }, Content = new TextBlock { Text = "Click: type. Drag: adjust. Arrows: hold to repeat." }, Padding = new Insets2(8) });
        var languages = new ComboBox
        {
            Height = 36
        };
        foreach (string name in new[]
        {
            "Lua",
            "Python",
            "C#",
            "Custom script"
        }

        )
            languages.Items.Add(new SelectorItem(name));
        right.Add(languages);
        var code = new ScriptTextBox
        {
            Language = ScriptLanguage.Lua,
            Text = "-- Lua example\nlocal speed = 1.25\nfunction update(dt)\n    return speed * dt\nend",
            Padding = new Insets2(12)
        };
        right.Add(code, row: 1);
        languages.SelectionChanged += (_, _) =>
        {
            if (languages.SelectedIndex == 3)
            {
                code.Highlighter = new GallerySyntaxHighlighter();
                code.Text = "-- Custom game script\nentity ball = \"player\"\nmove ball 10\nwait 0.5\nsay \"Ready\"";
                return;
            }

            code.Highlighter = null;
            code.Language = languages.SelectedIndex switch
            {
                1 => ScriptLanguage.Python,
                2 => ScriptLanguage.CSharp,
                _ => ScriptLanguage.Lua
            };
            code.Text = code.Language switch
            {
                ScriptLanguage.Python => "# Python example\ndef update(dt):\n    speed = 1.25\n    return speed * dt",
                ScriptLanguage.CSharp => "// C# example\npublic float Update(float dt)\n{\n    const float speed = 1.25f;\n    return speed * dt;\n}",
                _ => "-- Lua example\nlocal speed = 1.25\nfunction update(dt)\n    return speed * dt\nend"
            };
        };
        languages.SelectedIndex = 0;
        right.Add(new TextBlock { Text = "Editable syntax colors / Ctrl+Z, Ctrl+Y, selection, clipboard", Height = 32 }, row: 2);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (elapsed.TotalSeconds <= 0)
            return;
        _sampleTime += elapsed.TotalSeconds;
        _frames++;
        _frameTimes.Append((float)elapsed.TotalMilliseconds);
        if (_sampleTime >= .25)
        {
            _fps.Append((float)(_frames / _sampleTime));
            _frames = 0;
            _sampleTime = 0;
        }
    }
}
