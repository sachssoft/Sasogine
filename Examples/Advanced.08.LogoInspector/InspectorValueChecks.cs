using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Checks the complete registered catalog, exact numeric input, grouped geometry and custom registration.</summary>
internal static class InspectorValueChecks
{
    internal static int Run(ControlLayoutContext layout)
    {
        int checks = 0;
        void Check(bool valid, string message)
        {
            checks++;
            if (!valid)
                throw new InvalidOperationException(message);
        }

        var registry = new InspectorEditorRegistry();
        var catalog = InspectorValueCatalog.Create();
        Check(registry.Count == 76 && catalog.Count == 80, "Catalog covers explicit defaults and automatically supported enums");
        Check(registry.Names.Distinct().Count() == registry.Count, "Registration names are unique");
        var inspector = new InspectorView(registry);
        using var workspace = new Workspace(inspector);
        var frame = new ControlInputFrame(new Size2(900, 700), TimeSpan.Zero);
        void Frame() => workspace.Update(frame, layout);
        Frame();
        foreach (var example in catalog)
        {
            inspector.Clear();
            Frame();
            Check(workspace.Bindings.Count == 0, "Previous catalog bindings are detached");
            Control editor = example.Build(inspector);
            Frame();
            Check(editor.Workspace == workspace, "Catalog editor attaches: " + example.Name);
            Check(workspace.Bindings.Count == 1, "Catalog value has one typed binding: " + example.Name);
        }

        inspector.Clear();
        Frame();
        PixelPoint3 pixel = new(16777217, int.MinValue, int.MaxValue);
        var pixels = (TypedMultiValueEditor<PixelPoint3, int>)inspector.Add("Pixels", () => pixel, value => pixel = value);
        Frame();
        Check(pixels.Editors[0].Text == "16777217" && pixels.Editors[1].Text == "-2147483648", "Pixel components retain integers beyond Single precision");
        pixels.Editors[0].Text = "2147483647";
        Check(pixels.Editors[0].CommitEdit() && pixel.X == int.MaxValue && pixel.Y == int.MinValue && pixel.Z == int.MaxValue, "Int32 boundary edits preserve siblings");
        pixel = new(pixel.X, -16777217, pixel.Z);
        pixels.Editors[0].Text = "-5";
        pixels.Editors[0].CommitEdit();
        Check(pixel.X == -5 && pixel.Y == -16777217, "Typed integer edits preserve pending sibling changes");
        pixels.Editors[0].Text = "2147483648";
        Check(!pixels.Editors[0].CommitEdit() && pixels.Editors[0].HasError && pixel.X == -5, "Integer overflow does not modify the source");
        pixels.Editors[0].CancelEdit();
        Check(pixels.Editors[0].Text == "-5" && !pixels.Editors[0].HasError, "Cancellation restores committed integer text");
        inspector.Clear();
        Frame();
        ulong large = ulong.MaxValue;
        var unsigned = (TypedValueTextBox<ulong>)inspector.Add("Unsigned", () => large, value => large = value);
        Check(unsigned.Text == "18446744073709551615", "UInt64 formatting is exact");
        unsigned.Text = "18446744073709551614";
        Check(unsigned.CommitEdit() && large == ulong.MaxValue - 1, "UInt64 parsing is exact");
        double real = 1e300;
        var doubleEditor = (TypedValueTextBox<double>)inspector.Add("Double", () => real, value => real = value);
        doubleEditor.Text = "1.2345678901234567E+290";
        Check(doubleEditor.CommitEdit() && real == 1.2345678901234567e290, "Double values do not pass through Single or Decimal");
        doubleEditor.Text = "NaN";
        Check(!doubleEditor.CommitEdit() && real == 1.2345678901234567e290, "Nonfinite input is rejected without overwriting the source");
        doubleEditor.CancelEdit();
        float positive = 1;
        var validated = inspector.AddNumber("Positive", () => positive, value =>
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            positive = value;
        });
        validated.Text = "-1";
        Check(!validated.CommitEdit() && positive == 1 && validated.Value == 1 && validated.HasError, "Source validation preserves the last committed scalar");
        Point2 validatedPoint = new(1, 2);
        var validatedPair = (TypedMultiValueEditor<Point2, float>)inspector.Add("Validated point", () => validatedPoint, value =>
        {
            if (value.X < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            validatedPoint = value;
        });
        validatedPair.Editors[0].Text = "-1";
        Check(!validatedPair.Editors[0].CommitEdit() && validatedPoint.X == 1 && validatedPair.Value.X == 1 && validatedPair.Editors[0].Value == 1, "A rejected composite edit rolls back source and editor state");
        inspector.Clear();
        Frame();
        Bounds3 bounds = new(1, 2, 3, 100, 200, 300);
        var boundsEditor = (TypedMultiValueEditor<Bounds3, float>)inspector.Add("Bounds", () => bounds, value => bounds = value);
        Frame();
        Check(boundsEditor.Editors.Count == 6, "Bounds3 exposes all six coordinates and dimensions");
        Check(boundsEditor.Editors[0].Bounds.Y == boundsEditor.Editors[2].Bounds.Y && boundsEditor.Editors[3].Bounds.Y > boundsEditor.Editors[0].Bounds.Y, "Bounds3 groups Position and Size in separate horizontal rows");
        boundsEditor.Editors[4].Text = "456";
        boundsEditor.Editors[4].CommitEdit();
        Check(bounds.X == 1 && bounds.Y == 2 && bounds.Z == 3 && bounds.Width == 100 && bounds.Height == 456 && bounds.Depth == 300, "Bounds size editing preserves the complete position");
        Box3 box = new(1, 2, 3, 100, 200, 300);
        var boxEditor = (TypedMultiValueEditor<Box3, float>)inspector.Add("Box", () => box, value => box = value);
        Frame();
        boxEditor.Editors[2].Text = "-9";
        boxEditor.Editors[2].CommitEdit();
        Check(box.MinZ == -9 && box.MaxX == 100 && box.MaxY == 200 && box.MaxZ == 300, "Min editing preserves Max coordinates");
        Check(boxEditor.Editors[3].Bounds.Y > boxEditor.Editors[0].Bounds.Y, "Box Min and Max use distinct rows");
        inspector.Clear();
        Frame();
        Insets3 insets = new(1, 2, 3, 4, 5, 6);
        var insetEditor = (TypedMultiValueEditor<Insets3, float>)inspector.Add("Insets", () => insets, value => insets = value);
        Frame();
        insetEditor.Editors[4].Text = "17";
        insetEditor.Editors[4].CommitEdit();
        Check(insets.Left == 1 && insets.Top == 2 && insets.Front == 3 && insets.Right == 4 && insets.Bottom == 17 && insets.Back == 6, "Inset ordering matches the engine constructor");
        for (int i = 0; i < 300; i++)
            Frame();
        var allocationFrames = new long[1000];
        long before = GC.GetAllocatedBytesForCurrentThread(), previous = before;
        for (int i = 0; i < 1000; i++)
        {
            workspace.Update(frame, layout);
            long current = GC.GetAllocatedBytesForCurrentThread();
            allocationFrames[i] = current - previous;
            previous = current;
        }
        long allocated = previous - before;
        if (allocated != 0)
            Console.WriteLine("Allocation diagnostic: " + string.Join(", ", allocationFrames.Select((bytes, index) => (bytes, index)).Where(entry => entry.bytes != 0).Take(8)));
        Check(allocated == 0, "Steady polling of Insets3 must not box its struct; allocated bytes: " + allocated);
        Console.WriteLine($"BENCHMARK: 1000 steady typed Insets3 inspector updates: {allocated} bytes.");
        inspector.Clear();
        Frame();
        LowTieredScore<int> score = new(1, 5, 10);
        var expected = score.ChangeGold(-4);
        var scoreEditor = (TypedMultiValueEditor<LowTieredScore<int>, int>)inspector.Add("Score", () => score, value => score = value);
        scoreEditor.Editors[2].Text = "-4";
        scoreEditor.Editors[2].CommitEdit();
        Check(score.Bronze == expected.Bronze && score.Silver == expected.Silver && score.Gold == expected.Gold, "Tier score normalization remains defined by the engine");
        Check(scoreEditor.Editors[0].Value == score.Bronze && scoreEditor.Editors[2].Value == score.Gold, "Normalized score components refresh together");
        var custom = new InspectorEditorRegistry();
        custom.Register<Point2>("Point2 custom", static (view, title, read, write, source, name) => view.AddText(title, () => read().ToString(), _ =>
        {
        }, source, name));
        var customView = new InspectorView(custom);
        Point2 point = new(1, 2);
        Check(customView.Add("Point", () => point, value => point = value) is TextBox, "Custom registration is selected without runtime type inspection");
        Check(customView.AddGroup("Nested").Fields.Add("Point", () => point, value => point = value) is TextBox, "Nested groups share an explicitly supplied registry");
        Check(new InspectorView().Add("Point", () => point, value => point = value) is TypedMultiValueEditor<Point2, float>, "Custom registration does not modify global defaults");
        bool immutable = false;
        try
        {
            InspectorEditorRegistry.Default.Register<int>("Invalid", static (view, title, read, write, source, name) => view.AddNumber(title, read, write, source, name));
        }
        catch (InvalidOperationException)
        {
            immutable = true;
        }

        Check(immutable, "Built-in defaults cannot be globally mutated");
        bool missing = false;
        try
        {
            inspector.Add("Unsupported", () => new LogoDefinition(), _ =>
            {
            });
        }
        catch (NotSupportedException)
        {
            missing = true;
        }

        Check(missing, "Unregistered objects require an explicit factory rather than member discovery");
        Check(registry.Contains<DayOfWeek>() && registry.Contains<InspectorExampleMode>(), "Framework and application enums require no individual registration");
        DayOfWeek automaticDay = DayOfWeek.Monday;
        var automaticChoice = (ComboBox)inspector.Add("Automatic day", () => automaticDay, value => automaticDay = value);
        automaticChoice.SelectedIndex = 2;
        Check(automaticDay == DayOfWeek.Tuesday, "Automatic enum selection writes the declared value");
        automaticDay = (DayOfWeek)(-1);
        Frame();
        Check(automaticChoice.SelectedIndex == -1 && (int)automaticDay == -1, "Unknown enum values are preserved without choosing the first item");
        InspectorExampleMode mode = InspectorExampleMode.Read;
        var modeChoice = (ComboBox)inspector.Add("Automatic mode", () => mode, value => mode = value);
        Check(modeChoice.Items.Any(item => item.Text == "Both") && modeChoice.Items.Any(item => item.Text == "ReadWrite"), "Automatic choices preserve enum aliases");
        modeChoice.SelectedIndex = modeChoice.Items.ToList().FindIndex(item => item.Text == "HighBit");
        Check(mode == InspectorExampleMode.HighBit, "UInt64 enum high bits survive selection without signed conversion");
        mode = InspectorExampleMode.ReadWrite;
        Frame();
        Check(modeChoice.SelectedIndex >= 0, "Declared flag combinations are selectable");
        inspector.Clear();
        Frame();
        Check(workspace.Bindings.Count == 0, "Automatic enum bindings detach when cleared");
        DayOfWeek day = DayOfWeek.Monday;
        custom.RegisterChoices("Week", new[] { new KeyValuePair<string, DayOfWeek>("Monday", DayOfWeek.Monday), new("Tuesday", DayOfWeek.Tuesday) });
        var choice = (ComboBox)customView.Add("Day", () => day, value => day = value);
        choice.SelectedIndex = 1;
        Check(day == DayOfWeek.Tuesday && choice.Items.Count == 2, "Explicit enum choices override the automatic fallback");
        Check(workspace.Registry.InspectorEditors.Contains<PixelBox3>() && workspace.Registry.InspectorEditors.Contains<Insets2>(), "Workspace initializes the complete default editor registry");
        return checks;
    }
}
