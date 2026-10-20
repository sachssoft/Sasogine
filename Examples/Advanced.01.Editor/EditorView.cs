using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Tree, toolbar, custom canvas and explicitly wired property inspector.</summary>
internal sealed class EditorView
{
    internal EditorDocument Document { get; } = new();
    internal Workspace Workspace { get; }
    internal EditorCanvas Canvas { get; }

    private readonly TreeView _tree = new()
    {
    };
    private readonly TextBox _name = new()
    {
        Height = 36,
        MaxLength = 80
    };
    private readonly NumericSpinner[] _numbers = new NumericSpinner[4];
    private readonly CheckBox _visible = new()
    {
        Content = new TextBlock
        {
            Text = "Visible"
        },
        Height = 30
    };
    private readonly Button _color = new()
    {
        Content = new TextBlock
        {
            Text = "Change color..."
        },
        Height = 36
    };
    private readonly Label _status = new()
    {
        Height = 28
    };
    private readonly Label _file = new()
    {
        WrapText = true,
        Height = 64
    };
    private readonly Label _badge = new()
    {
        Text = "UNSAVED",
        Height = 28
    };
    private readonly RotationControl _rotation = new()
    {
        Height = 128,
        Width = 128,
        HorizontalAlignment = ControlAlignment.Center,
        ToolTip = "Drag to rotate. Shift snaps to 15°. Escape cancels. Arrow keys: 1°; Home: zero."
    };
    private readonly NumericSpinner _angle = new()
    {
        NumericMinimum = 0,
        NumericMaximum = 359.9m,
        NumericIncrement = 1,
        DecimalPlaces = 1,
        Height = 36
    };
    private bool _sync;
    private string _treeSignature = "";
    private string _path;
    internal EditorView(string path)
    {
        var page = new Grid()
        {
            Padding = new Insets2(16),
            Spacing = 10,
            Background = new Color(17, 24, 35)
        };
        page.Rows.Add(new RowTrack { Height = GridLength.Auto });
        page.Rows.Add(new RowTrack { Height = GridLength.Auto });
        page.Rows.Add(new RowTrack { Height = GridLength.Star() });
        page.Rows.Add(new RowTrack { Height = GridLength.Auto });
        Workspace = new Workspace(page);
        var registry = Workspace.Registry;
        _path = path;
        registry.Styles.Register("Editor.Card", new Style("Editor.Card", "Grid") { ["Background"] = "25,35,49", ["BorderColor"] = "56,71,92", ["BorderThickness"] = "1", ["Padding"] = "14", ["Spacing"] = "10" });
        registry.Styles.Register("Editor.Heading", new Style("Editor.Heading", "Label") { ["TextColor"] = "65,199,223", ["Height"] = "28" });
        registry.Styles.Register("Editor.Canvas", new Style("Editor.Canvas", "Control") { ["Background"] = "13,20,29", ["BorderColor"] = "56,71,92", ["BorderThickness"] = "1", ["IsFocusable"] = "true", ["ShowGrid"] = "true", ["SnapX"] = "20" });
        Canvas = new EditorCanvas(Document)
        {
            ToolTip = "Drag to move. Escape cancels; arrows move the selected object."
        };
        var menu = new MenuBar();
        page.Add(menu);
        var file = new MenuItem
        {
            Text = "&File"
        };
        menu.Items.Add(file);
        file.Items.Add(new MenuItem { Text = "New demo", ShortcutText = "Ctrl+N", ShortcutKey = Keys.N, ShortcutControl = true, Action = New });
        file.Items.Add(new MenuItem { Text = "Open JSON...", ShortcutText = "Ctrl+O", ShortcutKey = Keys.O, ShortcutControl = true, Action = Open });
        file.Items.Add(new MenuItem { Text = "Save JSON", ShortcutText = "Ctrl+S", ShortcutKey = Keys.S, ShortcutControl = true, Action = Save });
        var edit = new MenuItem
        {
            Text = "&Edit"
        };
        menu.Items.Add(edit);
        edit.Items.Add(new MenuItem { Text = "Undo document", ShortcutText = "Ctrl+Z", ShortcutKey = Keys.Z, ShortcutControl = true, Command = new Command(_ => Document.Undo(), _ => Document.CanUndo) });
        edit.Items.Add(new MenuItem { Text = "Redo document", ShortcutText = "Ctrl+Y", ShortcutKey = Keys.Y, ShortcutControl = true, Command = new Command(_ => Document.Redo(), _ => Document.CanRedo) });
        menu.Items.Add(new MenuItem { Text = "&Help", Action = () => _ = MessageBox.Show(Workspace, "Select in the tree or canvas. Drag rectangles; edit values on the right. Hold Shift with arrow keys for 20 px steps. A complete drag is one undo step. Save writes the JSON path shown in the inspector. Text fields keep their own undo history.", "Simple 2D editor") });
        var toolbar = new StackPanel()
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 8,
            Height = 36
        };
        page.Add(toolbar, row: 1);
        void Tool(string text, Action action, Func<bool>? allowed = null) => toolbar.Add(new Button() { Content = new TextBlock { Text = text }, Height = 36, Command = new Command(_ => action(), _ => !Document.IsEditing && (allowed?.Invoke() ?? true)) });
        Tool("+ Rectangle", () => Document.Add(), () => Document.Objects.Count < 256);
        Tool("Duplicate", () => Document.Add(true), () => Document.Selected is not null && Document.Objects.Count < 256);
        Tool("Delete", Document.Delete, () => Document.Selected is not null);
        Tool("Undo", Document.Undo, () => Document.CanUndo);
        Tool("Redo", Document.Redo, () => Document.CanRedo);
        Tool("Save", Save);
        var grid = new ToggleButton()
        {
            Content = new TextBlock
            {
                Text = "Grid"
            },
            IsChecked = true,
            Width = 72
        };
        toolbar.Add(grid);
        grid.CheckedChanged += (_, _) => Canvas.ShowGrid = grid.IsChecked;
        var snap = new ToggleButton()
        {
            Content = new TextBlock
            {
                Text = "Snap 20 px"
            },
            IsChecked = true,
            Width = 115
        };
        toolbar.Add(snap);
        snap.CheckedChanged += (_, _) => Canvas.Snap = snap.IsChecked;
        var body = new Grid()
        {
            Spacing = 12
        };
        page.Add(body, row: 2);
        body.Tracks.Add(new ColumnTrack { Width = 220 });
        body.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        body.Tracks.Add(new ColumnTrack { Width = 268 });
        var left = new Grid()
        {
            StyleId = "Editor.Card"
        };
        body.Add(left);
        left.Rows.Add(new RowTrack { Height = GridLength.Auto });
        left.Rows.Add(new RowTrack { Height = GridLength.Star() });
        left.Add(new Label() { StyleId = "Editor.Heading", Text = "SCENE OBJECTS" });
        left.Add(_tree, row: 1);
        var center = new Grid()
        {
            Spacing = 8
        };
        body.Add(center, column: 1);
        center.Rows.Add(new RowTrack { Height = GridLength.Auto });
        center.Rows.Add(new RowTrack { Height = GridLength.Star() });
        center.Rows.Add(new RowTrack { Height = GridLength.Auto });
        center.Add(new Label() { StyleId = "Editor.Heading", Text = "2D CANVAS  /  960 x 540" });
        center.Add(Canvas, row: 1);
        center.Add(new Label() { WrapText = true, Height = 48, Text = "Click selects. Drag moves. Escape cancels.\nArrow keys move; Shift uses 20 px steps." }, row: 2);
        var inspector = new StackPanel()
        {
            Spacing = 8,
            Padding = new Insets2(14),
            Background = new Color(25, 35, 49),
            BorderColor = new Color(56, 71, 92),
            BorderThickness = 1
        };
        body.Add(new ScrollViewer() { Content = inspector, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }, column: 2);
        inspector.Add(new Label() { StyleId = "Editor.Heading", Text = "PROPERTIES" });
        inspector.Add(new Label() { Text = "Name" });
        inspector.Add(_name);
        inspector.Add(new Label() { Text = "Rotation (degrees)" });
        var rotationRow = new Grid()
        {
            Height = 110,
            Spacing = 8
        };
        rotationRow.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        rotationRow.Tracks.Add(new ColumnTrack { Width = 110 });
        _angle.VerticalAlignment = ControlAlignment.Center;
        _rotation.Width = 110;
        _rotation.Height = 110;
        rotationRow.Add(_angle);
        rotationRow.Add(_rotation, column: 1);
        inspector.Add(rotationRow);
        _rotation.EditStarted += (_, _) => Document.BeginDrag();
        _rotation.AngleChanged += (_, _) =>
        {
            if (!_sync)
                Document.Rotate(_rotation.AngleDegrees);
        };
        _rotation.EditCompleted += (_, _) => Document.EndDrag(false);
        _rotation.EditCancelled += (_, _) => Document.EndDrag(true);
        _angle.ValueChanged += (_, _) =>
        {
            if (!_sync)
                Document.Rotate((float)_angle.Value);
        };
        string[] captions = ["Position X", "Position Y", "Width", "Height"];
        for (int j = 0; j < 4; j++)
        {
            int index = j;
            inspector.Add(new Label() { Text = captions[j] });
            var number = new NumericSpinner()
            {
                NumericMinimum = j < 2 ? 0 : 24,
                NumericMaximum = j % 2 == 0 ? 960 : 540,
                NumericIncrement = 1,
                DecimalPlaces = 0,
                Height = 36
            };
            _numbers[j] = number;
            inspector.Add(number);
            number.ValueChanged += (_, _) => ChangeNumber(index, (float)number.Value);
        }

        inspector.Add(_visible);
        inspector.Add(_color);
        inspector.Add(_badge);
        inspector.Add(new Label() { StyleId = "Editor.Heading", Text = "DOCUMENT FILE" });
        inspector.Add(_file);
        inspector.Add(new Label() { Text = "Open / Save: JSON version 2.\nMaximum 256 rectangles.", WrapText = true });
        page.Add(_status, row: 3);
        _tree.SelectionChanged += (_, _) =>
        {
            if (!_sync)
                Document.Select((_tree.SelectedItem?.Value as EditorObject)?.Id);
        };
        _name.TextChanged += (_, _) =>
        {
            if (!_sync && Document.Selected is EditorObject o && !string.IsNullOrWhiteSpace(_name.Text) && !_name.Text.Any(char.IsControl))
                Document.Update(o with { Name = _name.Text });
        };
        _visible.CheckedChanged += (_, _) =>
        {
            if (!_sync && Document.Selected is EditorObject o)
                Document.Update(o with { Visible = _visible.IsChecked == true });
        };
        _color.Clicked += (_, _) => PickColor();
        Document.Changed += Sync;
        if (File.Exists(path))
        {
            try
            {
                Document.Import(EditorFile.Load(path));
            }
            catch (Exception ex)when (IsFileError(ex))
            {
                _status.Text = "Could not load: " + ex.Message;
            }
        }

        Sync();
    }

    private void ChangeNumber(int i, float v)
    {
        if (_sync || Document.Selected is not EditorObject o)
            return;
        o = i switch
        {
            0 => o with
            {
                X = Math.Clamp(v, 0, 960 - o.Width)
            },
            1 => o with
            {
                Y = Math.Clamp(v, 0, 540 - o.Height)
            },
            2 => o with
            {
                Width = Math.Clamp(v, 24, 960 - o.X)
            },
            _ => o with
            {
                Height = Math.Clamp(v, 24, 540 - o.Y)
            }
        };
        Document.Update(o);
    }

    private void Sync()
    {
        _sync = true;
        try
        {
            string signature = string.Join("\n", Document.Objects.Select(o => o.Id + ":" + o.Name));
            if (_treeSignature != signature)
            {
                _treeSignature = signature;
                _tree.Items.Clear();
                var scene = new TreeViewItem
                {
                    Text = "Scene",
                    IsExpanded = true,
                    Icon = "icon_folder_open"
                };
                foreach (var item in Document.Objects)
                    scene.Items.Add(new TreeViewItem { Text = item.Name, Value = item });
                _tree.Items.Add(scene);
            }

            _tree.SelectedItem = _tree.Items.SelectMany(i => i.Items).FirstOrDefault(i => (i.Value as EditorObject)?.Id == Document.Selection && Document.Selection is not null);
            var o = Document.Selected;
            _name.IsEnabled = _visible.IsEnabled = _color.IsEnabled = o is not null;
            foreach (var number in _numbers)
                number.IsEnabled = o is not null;
            _rotation.IsEnabled = _angle.IsEnabled = o is not null;
            if (o is not null)
            {
                _rotation.AngleDegrees = o.RotationDegrees;
                _angle.Value = (decimal)o.RotationDegrees;
            }

            _name.Text = o?.Name ?? "";
            if (o is not null)
            {
                _numbers[0].NumericMaximum = (decimal)(960 - o.Width);
                _numbers[1].NumericMaximum = (decimal)(540 - o.Height);
                _numbers[2].NumericMaximum = (decimal)(960 - o.X);
                _numbers[3].NumericMaximum = (decimal)(540 - o.Y);
                float[] values = [o.X, o.Y, o.Width, o.Height];
                for (int i = 0; i < 4; i++)
                    _numbers[i].Value = (decimal)values[i];
                _visible.IsChecked = o.Visible;
                _color.Background = o.Color;
            }

            _badge.Text = Document.IsDirty ? "UNSAVED CHANGES" : "SAVED";
            _file.Text = Path.GetFileName(_path) + "\nHover for full path";
            _file.ToolTip = _path;
            _status.Text = $"{Document.Objects.Count} objects  |  {o?.Name ?? "No selection"}  |  {(Document.IsDirty ? "Modified" : "Saved")}  |  Ctrl+S saves JSON";
        }
        finally
        {
            _sync = false;
        }
    }

    private static bool IsFileError(Exception ex) => ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException or ArgumentException;
    private async void New()
    {
        if (Document.IsEditing)
            return;
        if (Document.IsDirty && await MessageBox.Show(Workspace, "Discard unsaved changes and create the demo scene?", "New scene", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;
        Workspace.Post(Document.New);
    }

    private async void Open()
    {
        if (Document.IsEditing)
            return;
        if (Document.IsDirty && await MessageBox.Show(Workspace, "Discard unsaved changes and open another document?", "Open scene", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;
        Workspace.Post(() => OpenDialog());
    }

    private async void OpenDialog()
    {
        var dialog = new OpenFileDialog()
        {
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(_path)) ? Path.GetDirectoryName(_path)! : AppContext.BaseDirectory
        };
        dialog.Filters.Add(new FileDialogFilter("Editor JSON", "*.json"));
        if (await dialog.ShowAsync(Workspace) != DialogResult.OK || dialog.SelectedPath is not string path)
            return;
        Workspace.Post(() =>
        {
            try
            {
                var objects = EditorFile.Load(path);
                _path = path;
                Document.Import(objects);
            }
            catch (Exception ex)when (IsFileError(ex))
            {
                ShowError(ex);
            }
        });
    }

    private void Save()
    {
        if (Document.IsEditing)
            return;
        try
        {
            EditorFile.Save(_path, Document.Objects);
            Document.MarkSaved();
            _status.Text = "Saved: " + _path;
        }
        catch (Exception ex)when (IsFileError(ex))
        {
            ShowError(ex);
        }
    }

    private void ShowError(Exception ex) => _ = MessageBox.Show(Workspace, ex.Message, "Document error");
    private async void PickColor()
    {
        if (Document.Selected is not EditorObject item)
            return;
        var dialog = new ColorPickerDialog()
        {
            SelectedColor = item.Color
        };
        if (await dialog.ShowAsync(Workspace) == DialogResult.OK)
            Workspace.Post(() =>
            {
                if (Document.Selected is EditorObject current && current.Id == item.Id)
                    Document.Update(current with { Color = dialog.SelectedColor });
            });
    }

    internal void Verify(ControlLayoutContext layout)
    {
        void Frame(params ControlInputEvent[] input) => Workspace.Update(new ControlInputFrame(new Size2(1280, 800), TimeSpan.FromSeconds(1.0 / 60), input), layout);
        Frame();
        var original = Document.Objects[0];
        Vector2 p = Canvas.Screen(original.X + 40, original.Y + 40), q = Canvas.Screen(original.X + 100, original.Y + 80);
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, p));
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, q));
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, q));
        if (Document.Selected?.X != original.X + 60 || Document.Selected.Y != original.Y + 40)
            throw new InvalidOperationException("Canvas drag did not update inspector/model.");
        Document.Undo();
        if (Document.Objects[0] != original)
            throw new InvalidOperationException("Drag undo failed.");
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, p));
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, q));
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
        if (Document.Objects[0] != original || Document.IsEditing)
            throw new InvalidOperationException("Escape rollback failed.");
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, p));
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, q));
        Frame(new ControlInputEvent(ControlInputEventType.Deactivate));
        if (Document.Objects[0] != original || Document.IsEditing)
            throw new InvalidOperationException("Capture cancellation failed.");
        // Another touch cannot steal an active drag.
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, p, PointerId: 7, PointerKind: ControlPointerKind.Touch));
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, q, PointerId: 8, PointerKind: ControlPointerKind.Touch));
        if (Document.Objects[0] != original)
            throw new InvalidOperationException("Foreign touch changed the drag.");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, q, PointerId: 7, PointerKind: ControlPointerKind.Touch));
        if (Document.Objects[0].X != original.X + 60)
            throw new InvalidOperationException("Touch release coordinates lost.");
        Document.Undo();
        _numbers[0].Value = 120;
        if (Document.Selected?.X != 120)
            throw new InvalidOperationException("Numeric inspector edit failed.");
        Document.Undo();
        _angle.Value = 30;
        if (Document.Selected?.RotationDegrees != 30)
            throw new InvalidOperationException("Rotation inspector/model synchronization failed.");
        // The inspector scrolls; directly test the model transaction here, while standalone UI tests cover dial input.
        Document.BeginDrag();
        Document.Rotate(45);
        Document.Rotate(75);
        Document.EndDrag(false);
        Document.Undo();
        if (Document.Selected?.RotationDegrees != 30)
            throw new InvalidOperationException("Grouped rotation undo failed.");
        Vector2 rotated = EditorGeometry.Rotate(new Vector2(original.X + 20, original.Y + 20), EditorGeometry.Center(original), 30);
        if (!EditorGeometry.Contains(Document.Selected!, rotated))
            throw new InvalidOperationException("Rotated object picking failed.");
        Frame();
        Console.WriteLine("PASS: rotation inspector, grouped undo and inverse rotated hit testing.");
        Console.WriteLine("PASS: editor canvas selection, captured drag, inspector synchronization/edits, independent touch, grouped undo, Escape and deactivation rollback.");
    }
}
