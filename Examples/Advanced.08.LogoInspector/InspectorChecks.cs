using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Checks two-way editing, nested groups, tracked mutations, cache stability and binding lifetime.</summary>
internal static class InspectorChecks
{
    private static int _checks;
    private static void Check(bool condition, string message)
    {
        _checks++;
        if (!condition)
            throw new InvalidOperationException(message);
    }

    internal static void Run()
    {
        using var view = new LogoInspectorView();
        Verify(view, new ControlLayoutContext(new InspectorProbeFont()));
    }

    internal static void Verify(LogoInspectorView view, ControlLayoutContext layout)
    {
        _checks = 0;
        void Frame(params ControlInputEvent[] events)
        {
            view.Workspace.Update(new ControlInputFrame(new Size2(1360, 900), TimeSpan.FromSeconds(.016), events), layout);
            view.Update();
        }

        Frame();
        Frame();
        Check(view.Entity is EntityBase<LogoDefinition> && view.Definition is IEntityDefinition, "Real engine entity and definition");
        Check(view.Entity.Segments.Count > 8, "Prepared vector logo");
        int revision = view.Entity.GeometryRevision;
        int metrics = view.Canvas.MetricsRevision;
        for (int i = 0; i < 20; i++)
            Frame();
        Check(view.Entity.GeometryRevision == revision, "Unchanged geometry stays cached");
        Check(view.Canvas.MetricsRevision == metrics, "Unchanged title metrics stay cached");
        view.ScaleEditor.Editors[0].Value = 1.2f;
        Frame();
        Check(MathF.Abs(view.Definition.Transform.Scale.X - 1.2f) < .011f && view.Definition.Transform.Scale.Y == 1, "Component editor changes X while retaining Y");
        Check(view.Entity.GeometryRevision > revision, "Editing rebuilds geometry");
        view.Definition.Transform.Scale = new(.75f, 1.25f);
        Frame();
        Check(view.ScaleEditor.Value == new Vector2(.75f, 1.25f), "External notifications update the composite editor");
        Check(view.TransformGroup.IsExpanded, "Nested expander state survives value changes");
        Check(view.PositionEditor.Editors.Count == 2 && view.SpatialPositionEditor.Editors.Count == 3, "2D and 3D properties expose complete component rows");
        Check(view.PositionEditor.Editors[0].Bounds.Y == view.PositionEditor.Editors[1].Bounds.Y, "X and Y share one row");
        Check(view.SpatialPositionEditor.Editors.All(editor => editor.Bounds.Y == view.SpatialPositionEditor.Editors[0].Bounds.Y), "X Y Z share one row");
        Check(view.SpatialPositionEditor.Editors[0].Bounds.Right < view.SpatialPositionEditor.Editors[1].Bounds.X, "Component fields have separate horizontal bounds");
        var originalX = view.PositionEditor.Editors[0];
        int vectorNotifications = 0;
        PropertyChangedEventHandler onVector = (_, args) =>
        {
            if (args.PropertyName == nameof(LogoSpatialDefinition.Position))
                vectorNotifications++;
        };
        view.Definition.Spatial.PropertyChanged += onVector;
        view.SpatialPositionEditor.Editors[2].Value = 45;
        Check(view.Definition.Spatial.Position == new Vector3(0, 0, 45) && vectorNotifications == 1, "A component edit commits one complete vector notification");
        // An external sibling change may arrive before the next binding pass.
        view.Definition.Spatial.Position = new(0, 27, 45);
        view.SpatialPositionEditor.Editors[0].Value = 16;
        Check(view.Definition.Spatial.Position == new Vector3(16, 27, 45), "Editing X preserves a pending external Y change");
        Frame();
        Check(view.SpatialPositionEditor.Value == new Vector3(16, 27, 45) && view.SpatialPositionEditor.Editors[1].Value == 27, "All component fields refresh after the atomic write");
        view.Definition.Transform.Position = new(12, -8);
        Frame();
        Check(ReferenceEquals(originalX, view.PositionEditor.Editors[0]), "Vector updates retain existing editor controls");
        Check(view.Entity.Offset == new Vector2(12, -8), "2D position updates the actual entity");
        var projected = view.Entity.Segments[0].Start;
        view.SpatialRotationEditor.Editors[1].Value = 40;
        Frame();
        Check(view.Entity.Segments[0].Start != projected, "3D rotation changes the projected logo geometry");
        view.SpatialScaleEditor.Editors[2].Value = 1.25f;
        Check(view.Definition.Spatial.Scale == new Vector3(1, 1, 1.25f), "3D scale retains sibling components");
        view.Definition.Spatial.PropertyChanged -= onVector;
        // Use routed keyboard input on an actual visible text editor.
        view.Workspace.Focus(view.TitleEditor);
        view.TitleEditor.SelectAll();
        Frame("LIVE LOGO".Select(character => new ControlInputEvent(ControlInputEventType.TextInput, Character: character)).ToArray());
        Check(view.Definition.Title == "LIVE LOGO" && view.Entity.Title == "LIVE LOGO", "Routed text input reaches definition and entity");
        int count = view.Definition.Accents.Count;
        int version = view.Definition.Accents.Version;
        view.AccentEditor.AddItem();
        Frame();
        Frame();
        Check(view.Definition.Accents.Count == count + 1 && view.Definition.Accents.Version > version, "Add retains tracking");
        var added = view.AccentEditor.SelectedItem;
        Check(ReferenceEquals(added, view.Definition.Accents[^1]), "New item is selected");
        view.AccentEditor.MoveSelected(-1);
        Frame();
        Frame();
        Check(ReferenceEquals(added, view.AccentEditor.SelectedItem) && view.AccentEditor.SelectedIndex == count - 1, "Move retains selected identity");
        view.TransformGroup.IsExpanded = false;
        Frame();
        view.Definition.Appearance.Stroke = 16;
        Frame();
        Check(!view.TransformGroup.IsExpanded, "Unrelated edits preserve collapsed groups");
        view.AccentEditor.RemoveSelected();
        Frame();
        Frame();
        Check(view.Definition.Accents.Count == count, "Remove uses collection mutation");
        while (view.Definition.Accents.Count > 0)
        {
            view.AccentEditor.RemoveSelected();
            Frame();
            Frame();
        }

        Check(view.AccentEditor.SelectedItem is null && view.AccentEditor.List.SelectedIndex == -1, "Empty selection is valid");
        view.AccentEditor.AddItem();
        Frame();
        Frame();
        Check(view.AccentEditor.SelectedItem is not null, "Empty list accepts another item");
        view.Definition.Visible = false;
        Frame();
        Check(!view.Entity.IsVisible, "Visibility reaches the entity");
        view.Definition.Visible = true;
        // Restore the initial presentation after checking edits.
        view.Definition.Title = "SASOGINE";
        view.Definition.Transform.Scale = Vector2.One;
        view.Definition.Transform.Position = Vector2.Zero;
        view.Definition.Spatial.Position = Vector3.Zero;
        view.Definition.Spatial.Rotation = Vector3.Zero;
        view.Definition.Spatial.Scale = Vector3.One;
        view.Definition.Appearance.Stroke = 12;
        view.Definition.Accents.Clear();
        view.Definition.Accents.Add(new LogoAccentDefinition { Name = "Upper orbit", Angle = -35 });
        view.Definition.Accents.Add(new LogoAccentDefinition { Name = "Lower orbit", Angle = 145, Length = 45 });
        view.TransformGroup.IsExpanded = true;
        Frame();
        Frame();
        view.Workspace.Focus(null);
        Check(view.Entity.Title == "SASOGINE" && view.Entity.IsVisible, "Initial display restored");
        // Detached inspectors release workspace bindings and reconnect on another owner.
        var probe = new InspectorView();
        int reads = 0;
        float model = 1;
        var source = new LogoTransformDefinition();
        var number = probe.AddSingle("Probe", () =>
        {
            reads++;
            return source.Rotation;
        }, v => source.Rotation = v, -180, 180, .01f, source, nameof(source.Rotation));
        var first = new Workspace(probe);
        first.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero), layout);
        int stable = reads;
        for (int i = 0; i < 20; i++)
            first.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero), layout);
        Check(reads == stable, "Notification-driven values do not poll unchanged getters");
        first.Dispose();
        Check(first.Bindings.Count == 0 && probe.Workspace is null, "Detach releases binding ownership");
        source.Rotation = 1.3f;
        using (var second = new Workspace(probe))
        {
            second.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero), layout);
            Check(MathF.Abs(number.Value - 1.3f) < .011f, "Reattach refreshes changes made while detached");
            var polled = probe.AddSingle("Polling", () => model, v => model = v, 0, 10);
            model = 5;
            second.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero), layout);
            Check(polled.Value == 5, "Plain objects support explicit polling");
            Vector4 vector4 = new(1, 2, 3, 4);
            var four = probe.AddVector4("Vector4", () => vector4, value => vector4 = value, -100, 100);
            four.Editors[3].Value = 9;
            Check(vector4 == new Vector4(1, 2, 3, 9), "Four-component edits retain X Y Z");
            vector4 = new(4, 5, 6, 7);
            second.Update(new ControlInputFrame(new Size2(800, 700), TimeSpan.Zero), layout);
            Check(four.Value == vector4 && four.Editors[0].Value == 4 && four.Editors[3].Value == 7, "Vector4 polling refreshes all fields");
            Size2 dimensions = new(10, 20);
            var size = probe.AddValues("Size", () => dimensions, value => dimensions = value, new[] { new ValueComponent<Size2>("W", static value => value.Width, static (value, width) => new(width, value.Height), 1, 100), new ValueComponent<Size2>("H", static value => value.Height, static (value, height) => new(value.Width, height), 1, 100) });
            size.Editors[0].Value = 35;
            Check(dimensions.Width == 35 && dimensions.Height == 20, "Typed components support engine structs without discovery");
            Check(size.Editors[0].Value == 35 && size.Editors[1].Value == 20, "Custom components retain their values");
            Color color = Color.Cyan;
            var picker = probe.AddColor("Color", () => color, value => color = value);
            second.Update(new ControlInputFrame(new Size2(800, 700), TimeSpan.Zero), layout);
            ColorPickerDialog OpenColor()
            {
                picker.PerformClick();
                Control? control = second.FocusedControl;
                while (control is not null && control is not ColorPickerDialog)
                    control = control.Parent;
                return control as ColorPickerDialog ?? throw new InvalidOperationException("Color picker did not open.");
            }

            var cancel = OpenColor();
            cancel.SelectedColor = Color.Red;
            cancel.Complete(DialogResult.Cancel);
            Check(color == Color.Cyan && second.OverlayCount == 0, "Color cancellation preserves the source");
            var accept = OpenColor();
            accept.SelectedColor = Color.Red;
            accept.Complete(DialogResult.OK);
            Check(color == Color.Red, "Color confirmation writes on the workspace thread");
            var stale = OpenColor();
            probe.Clear();
            second.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero), layout);
            Check(second.Bindings.Count == 0, "Clearing fields releases their bindings");
            stale.SelectedColor = Color.Blue;
            stale.Complete(DialogResult.OK);
            Check(color == Color.Red, "A removed color field cannot commit a stale dialog");
        }

        view.MenuVisibility.IsChecked = false; Frame();
        Check(!view.ScaleEditor.Parent!.Children.OfType<InspectorPropertyMenu>().Single().IsVisible && !view.AssetGallery.Inspector.ShowPropertyMenus,
            "Example checkbox controls inspector menu visibility across tabs");
        view.MenuVisibility.IsChecked = true; Frame();
        Check(view.ScaleEditor.Parent!.Children.OfType<InspectorPropertyMenu>().Single().IsVisible,
            "Example checkbox restores inspector property menus");
        _checks += InspectorValueChecks.Run(layout);
        _checks += AssetControlsChecks.Run(layout);
        _checks += InspectorPropertyMenuChecks.Run(layout);
        Console.WriteLine($"PASS: {_checks} inspector checks; registered value catalog, exact numeric input, grouping, cache and binding lifetime.");
    }
}
