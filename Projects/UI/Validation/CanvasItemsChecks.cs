using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Runs deterministic, graphics-device-free checks of item reconciliation and canvas gestures.</summary>
internal static class CanvasItemsChecks
{
    /// <summary>Runs collection, ownership and geometry checks.</summary>
    internal static void Run()
    {
        var font = new CanvasItemsTestFont();
        var layout = new ControlLayoutContext(font);
        var a = new object();
        var b = new object();
        var c = new object();
        var source = new ObservableCollection<object> { a, b };
        var host = new ItemsControl { ItemsSource = source };
        int creations = 0;
        host.ItemTemplate = new DelegateTemplate<object?, Control>(item =>
        {
            creations++;
            return new CanvasItem(item) { Content = new TextBlock { Text = "Item" }, Position = new Point2(40, 40) };
        });
        host.ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(static _ => new Canvas());
        using var workspace = new Workspace(host);
        void Frame(params ControlInputEvent[] events)
            => workspace.Update(new ControlInputFrame(new Size2(800, 600), TimeSpan.Zero, events), layout);
        Frame();
        Check(host.ItemCount == 2 && creations == 2, "Initial source creates exactly two presentations.");
        Control first = host.ContainerFromIndex(0);
        Control second = host.ContainerFromIndex(1);
        source.Move(0, 1);
        Frame();
        Check(ReferenceEquals(host.ContainerFromIndex(0), second) && ReferenceEquals(host.ContainerFromIndex(1), first)
            && ReferenceEquals(host.ItemsPanelRoot.Children[0], second) && creations == 2, "Move retains presentations and updates panel order.");
        source.Insert(1, c);
        Frame();
        Check(creations == 3 && ReferenceEquals(host.ContainerFromIndex(2), first), "Insert creates only the new item.");
        source[1] = new object();
        Frame();
        Check(creations == 4 && host.ItemCount == 3, "Replace recreates only the replaced item.");
        source.RemoveAt(1);
        Frame();
        Check(creations == 4 && host.ItemCount == 2, "Remove retains other presentations.");
        host.ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(static _ => new Canvas());
        Frame();
        Check(ReferenceEquals(host.ContainerFromIndex(1), first) && ReferenceEquals(first.Parent, host.ItemsPanelRoot), "Panel template change transfers existing presentations.");
        bool rejected = false;
        try { host.Items.Add(new object()); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "Direct items cannot contradict an external source.");

        var canvas = (Canvas)host.ItemsPanelRoot;
        var item = (CanvasItem)first;
        ((CanvasItem)second).IsVisible = false;
        canvas.SelectedItem = item;
        Frame();
        Vector2 Body() => Vector2.Transform(new Vector2(item.Bounds.X + 25, item.Bounds.Y + 30), item.VisualTransform);
        Vector2 point = Body();
        Frame(new(ControlInputEventType.PointerDown, point), new(ControlInputEventType.PointerMove, point + new Vector2(30, 20)),
            new(ControlInputEventType.PointerUp, point + new Vector2(30, 20)));
        Check(Near(item.Position.X, 70) && Near(item.Position.Y, 60), "Body drag moves a generic item.");
        point = Body();
        Frame(new(ControlInputEventType.PointerDown, point), new(ControlInputEventType.PointerMove, point + new Vector2(50, 10)),
            new(ControlInputEventType.KeyDown, Key: Keys.Escape));
        Check(Near(item.Position.X, 70) && Near(item.Position.Y, 60) && workspace.CapturedControl is null, "Escape restores the complete transform and releases capture.");
        item.Rotation = .6f;
        item.Scale = new Vector2(1.5f, .8f);
        Frame();
        Vector2 oldCorner = Vector2.Transform(new Vector2(item.Bounds.X, item.Bounds.Y), item.VisualTransform);
        Vector2 corner = Vector2.Transform(new Vector2(item.Bounds.Right, item.Bounds.Bottom), item.VisualTransform);
        Vector2 delta = Vector2.TransformNormal(new Vector2(30, 20), Matrix.CreateScale(item.Scale.X, item.Scale.Y, 1) * Matrix.CreateRotationZ(item.Rotation));
        Frame(new(ControlInputEventType.PointerDown, corner), new(ControlInputEventType.PointerMove, corner + delta),
            new(ControlInputEventType.PointerUp, corner + delta));
        Check(Near(item.Size.Width, 130) && Near(item.Size.Height, 120), "Resize uses the inverse rotated/scaled pointer transform.");
        Vector2 newCorner = Vector2.Transform(new Vector2(item.Bounds.X, item.Bounds.Y), item.VisualTransform);
        Check(Vector2.Distance(oldCorner, newCorner) < .02f, "Resize preserves the opposite visual corner.");

        Vector2 oldSize = new(item.Size.Width, item.Size.Height);
        corner = Vector2.Transform(new Vector2(item.Bounds.Right, item.Bounds.Bottom), item.VisualTransform);
        Frame(new(ControlInputEventType.PointerDown, corner, Alt: true), new(ControlInputEventType.PointerMove, corner + delta, Alt: true),
            new(ControlInputEventType.PointerUp, corner + delta, Alt: true));
        Check(Near(item.Size.Width, oldSize.X) && Near(item.Size.Height, oldSize.Y) && item.Scale.X > 1.5f, "Scaling leaves layout dimensions unchanged.");

        Vector2 visibleBefore = Vector2.Transform(new Vector2(item.Bounds.X, item.Bounds.Y), item.VisualTransform);
        Vector2 pivot = new(item.Bounds.X + item.Bounds.Width * item.RotationPivot.X, item.Bounds.Y + item.Bounds.Height * item.RotationPivot.Y);
        Frame(new(ControlInputEventType.PointerDown, pivot), new(ControlInputEventType.PointerMove, pivot + new Vector2(15, 9)),
            new(ControlInputEventType.PointerUp, pivot + new Vector2(15, 9)));
        Vector2 visibleAfter = Vector2.Transform(new Vector2(item.Bounds.X, item.Bounds.Y), item.VisualTransform);
        Check(Vector2.Distance(visibleBefore, visibleAfter) < .02f, "Moving the pivot preserves visual placement.");
        float rotationBefore = item.Rotation;
        Vector2 upper = Vector2.Transform(new Vector2(item.Bounds.X + item.Bounds.Width / 2, item.Bounds.Y), item.VisualTransform);
        Vector2 rotateHandle = upper + new Vector2(MathF.Sin(item.Rotation), -MathF.Cos(item.Rotation)) * 24;
        pivot = new Vector2(item.Bounds.X + item.Bounds.Width * item.RotationPivot.X, item.Bounds.Y + item.Bounds.Height * item.RotationPivot.Y);
        Vector2 rotatedPointer = pivot + Vector2.Transform(rotateHandle - pivot, Matrix.CreateRotationZ(.4f));
        Frame(new(ControlInputEventType.PointerDown, rotateHandle), new(ControlInputEventType.PointerMove, rotatedPointer),
            new(ControlInputEventType.PointerUp, rotatedPointer));
        Check(Near(item.Rotation, rotationBefore + .4f), "Rotation handle rotates around the edited pivot.");
        canvas.IsEditingEnabled = false;
        var button = new Button { Content = "Click" };
        int clicks = 0;
        button.Clicked += (_, _) => clicks++;
        item.Content = button;
        Frame();
        point = Body();
        Frame(new(ControlInputEventType.PointerDown, point), new(ControlInputEventType.PointerUp, point));
        Check(clicks == 1, "Arbitrary child controls receive inverse-transformed pointer input.");
        source.Clear();
        Frame();
        Check(host.ItemCount == 0 && canvas.SelectedItem is null, "Reset removes presentations and clears invalid selection.");
        Console.WriteLine("All controls checks passed.");
    }

    /// <summary>Checks a deterministic assertion.</summary>
    /// <param name="condition">Whether the assertion holds.</param>
    /// <param name="message">The assertion description.</param>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Console.WriteLine("PASS " + message);
    }

    /// <summary>Compares floating point geometry with a small tolerance.</summary>
    /// <param name="a">The actual value.</param>
    /// <param name="b">The expected value.</param>
    /// <returns>Whether both values are close.</returns>
    private static bool Near(float a, float b) => MathF.Abs(a - b) < .02f;
}

