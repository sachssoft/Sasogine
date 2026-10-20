using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Exercises edit transactions and versioned documents without a graphics device.</summary>
internal static class EditorChecks
{
    internal static void Run()
    {
        int checks = 0;
        void Require(bool ok, string message)
        {
            checks++;
            if (!ok)
                throw new InvalidOperationException(message);
        }

        var d = new EditorDocument();
        d.MarkSaved();
        var initial = d.Objects.ToArray();
        d.Add();
        Require(d.Objects.Count == 4 && d.IsDirty, "Add");
        d.Undo();
        Require(d.Objects.SequenceEqual(initial) && !d.IsDirty, "Undo/dirty checkpoint");
        d.Redo();
        Require(d.Objects.Count == 4, "Redo");
        d.Delete();
        Require(d.Objects.Count == 3, "Delete");
        d.Select(1);
        d.Add(true);
        Require(d.Selected?.Name == "Welcome card copy", "Duplicate");
        d.Undo();
        d.Select(1);
        var item = d.Selected!;
        d.BeginDrag();
        for (int i = 0; i < 20; i++)
            d.Drag(item.X + i, item.Y + i);
        d.EndDrag(false);
        Require(d.Selected!.X == item.X + 19, "Live drag");
        d.Undo();
        Require(d.Selected == item, "Grouped drag undo");
        d.BeginDrag();
        d.Drag(9999, -999);
        Require(d.Selected!.X == 960 - item.Width && d.Selected.Y == 0, "Drag bounds");
        d.EndDrag(true);
        Require(d.Selected == item, "Cancel transaction");
        byte[] bytes = EditorFile.Encode(d.Objects);
        var roundtrip = EditorFile.Decode(bytes);
        Require(roundtrip.SequenceEqual(d.Objects), "JSON round trip");
        bool rejected = false;
        try
        {
            EditorFile.Decode(System.Text.Encoding.UTF8.GetBytes("{\"version\":3,\"objects\":[]}"));
        }
        catch (InvalidDataException)
        {
            rejected = true;
        }

        Require(rejected, "Version validation");
        rejected = false;
        try
        {
            d.Import([item, item]);
        }
        catch (InvalidDataException)
        {
            rejected = true;
        }

        Require(rejected && d.Selected == item, "Invalid import is atomic");
        rejected = false;
        try
        {
            d.Update(item with { X = float.NaN });
        }
        catch (InvalidDataException)
        {
            rejected = true;
        }

        Require(rejected && d.Selected == item, "Reject nonfinite bounds");
        d.Import(roundtrip);
        Require(!d.IsDirty, "Load checkpoint");
        d.Update(item with { Color = Color.Red, Visible = false });
        Require(d.IsDirty && d.Selected!.Color == Color.Red && !d.Selected.Visible, "Inspector values");
        d.Undo();
        Require(!d.IsDirty, "Undo to saved state");
        d.Add();
        Require(!d.CanRedo, "New edit invalidates redo");
        d.Select(1);
        var unrotated = d.Selected!;
        d.BeginDrag();
        d.Rotate(30);
        d.Rotate(90);
        d.EndDrag(false);
        Require(d.Selected!.RotationDegrees == 90, "Rotation transaction");
        d.Undo();
        Require(d.Selected == unrotated, "Rotation undo");
        d.Redo();
        Require(d.Selected!.RotationDegrees == 90, "Rotation redo");
        Require(EditorFile.Decode(EditorFile.Encode(d.Objects)).SequenceEqual(d.Objects), "Rotation JSON round trip");
        var rotated = d.Selected!;
        Vector2 center = EditorGeometry.Center(rotated);
        Require(EditorGeometry.Contains(rotated, center), "Rotated hit test center");
        Vector2 test = EditorGeometry.Rotate(new Vector2(rotated.X + 5, rotated.Y + 5), center, 90);
        Require(EditorGeometry.Contains(rotated, test), "Inverse rotated picking");
        Require(!EditorGeometry.Contains(rotated, center + new Vector2(10000, 10000)), "Rotated hit miss");
        var legacy = EditorFile.Decode(System.Text.Encoding.UTF8.GetBytes("{\"version\":1,\"objects\":[{\"id\":1,\"name\":\"Old\",\"x\":0,\"y\":0,\"width\":100,\"height\":50,\"rgba\":4294967295,\"visible\":true}]}"));
        Require(legacy[0].RotationDegrees == 0, "Read version 1 documents");
        string folder = Path.Combine(Path.GetTempPath(), "Sasogine-Editor-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(folder, "document.json");
        try
        {
            EditorFile.Save(path, d.Objects);
            Require(EditorFile.Load(path).SequenceEqual(d.Objects), "File save/load");
            EditorFile.Save(path, []);
            Require(EditorFile.Load(path).Length == 0, "File replacement");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
            if (Directory.Exists(folder))
                Directory.Delete(folder);
        }

        Console.WriteLine($"PASS: {checks} document checks; editing, undo/redo, rollback, validation and JSON file round trip.");
    }
}
