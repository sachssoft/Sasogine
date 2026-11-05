using System.ComponentModel;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Components.Tools;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Layouts;

/// <summary>Positions controls freely and provides editing handles for arbitrary CanvasItem content.</summary>
/// <remarks>Use this panel as an ItemsControl.ItemsPanel template. Ordinary controls remain at the
/// content origin; CanvasItem exposes editable geometry. Drag the body to move, edges/corners to
/// resize, Alt-drag a size handle to scale, the upper round handle to rotate, and the cross to move
/// the pivot. Shift preserves proportions or snaps rotation to 15 degrees. Escape cancels a gesture.</remarks>
public class Canvas : ContainerControl, IControlAdornment
{
    /// <summary>Identifies changes to editing availability.</summary>
    private static readonly PropertyChangedEventArgs IsEditingEnabledArgs = new(nameof(IsEditingEnabled));

    /// <summary>Identifies selection changes.</summary>
    private static readonly PropertyChangedEventArgs SelectedItemArgs = new(nameof(SelectedItem));

    /// <summary>Stores whether pointer editing is enabled.</summary>
    private bool _isEditingEnabled = true;

    /// <summary>Stores whether an editor gesture is active.</summary>
    private bool _dragging;

    /// <summary>Stores the selected editable child.</summary>
    private CanvasItem? _selectedItem;

    /// <summary>Stores the active transformation kind.</summary>
    private CanvasEditOperation _operation;

    /// <summary>Stores the pointer captured for the current gesture.</summary>
    private ControlPointer _pointer;

    /// <summary>Stores accumulated rotation to avoid discontinuities at the angle wrap.</summary>
    private float _angle;

    /// <summary>Stores the preceding pointer angle.</summary>
    private float _lastAngle;

    /// <summary>Stores the active resize handle, numbered clockwise from the upper-left corner.</summary>
    private int _handle;

    /// <summary>Stores the inverse visual transform at the start of a gesture.</summary>
    private Matrix _startInverse;

    /// <summary>Stores the original item transform for cancellation and undo notifications.</summary>
    private Transform2State? _start;

    /// <summary>Stores the initial pointer in canvas layout coordinates.</summary>
    private Vector2 _startPointer;

    /// <summary>Occurs at the start, during updates, and at completion or cancellation of a gesture.</summary>
    public event EventHandler<CanvasTransformEventArgs>? TransformChanged;

    /// <summary>Creates a free-positioning panel with editor gestures enabled.</summary>
    public Canvas()
    {
        IsFocusable = true;
    }

    /// <summary>Gets or sets the selected direct child. Null clears the selection.</summary>
    public CanvasItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(value, _selectedItem))
                return;
            if (value is not null && !ReferenceEquals(value.Parent, this))
                throw new InvalidOperationException("Only a direct child of this canvas can be selected.");
            FinishGesture(true);
            SetNotify(ref _selectedItem, value, SelectedItemArgs);
        }
    }

    /// <summary>Gets or sets whether selecting and transforming items through input is enabled.</summary>
    public bool IsEditingEnabled
    {
        get => _isEditingEnabled;
        set
        {
            VerifyTreeMutation();
            if (!SetNotify(ref _isEditingEnabled, value, IsEditingEnabledArgs))
                return;
            if (!value)
                FinishGesture(true);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(Canvas);

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <summary>Draws selection geometry after item content in canvas-local coordinates.</summary>
    /// <param name="context">The current canvas drawing context.</param>
    public void DrawAdornment(ControlDrawContext context)
    {
        if (!IsEditingEnabled || _selectedItem is not CanvasItem item || !ReferenceEquals(item.Parent, this) || !item.IsVisible)
            return;
        Color color = item.IsLocked ? new Color(140, 140, 140) : new Color(70, 170, 255);
        Vector2 a = HandlePosition(item, 0);
        Vector2 b = HandlePosition(item, 2);
        Vector2 c = HandlePosition(item, 4);
        Vector2 d = HandlePosition(item, 6);
        context.DrawLine(a, b, color);
        context.DrawLine(b, c, color);
        context.DrawLine(c, d, color);
        context.DrawLine(d, a, color);
        if (item.IsLocked)
            return;
        if (item.AllowResize || item.AllowScale)
            for (int i = 0; i < 8; i++)
                DrawHandle(context, HandlePosition(item, i), color);
        if (item.AllowRotate)
        {
            Vector2 rotation = RotationHandle(item);
            context.DrawLine(HandlePosition(item, 1), rotation, color);
            DrawHandle(context, rotation, color);
        }
        if (item.AllowPivot)
        {
            Vector2 pivot = PivotPosition(item);
            context.DrawLine(pivot - new Vector2(6, 0), pivot + new Vector2(6, 0), color);
            context.DrawLine(pivot - new Vector2(0, 6), pivot + new Vector2(0, 6), color);
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        float width = 0;
        float height = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, new Size2(float.PositiveInfinity, float.PositiveInfinity));
            if (!child.IsVisible)
                continue;
            Point2 position = child is CanvasItem item ? item.Position : default;
            Bounds2 rectangle = new(position.X, position.Y, child.DesiredSize.Width, child.DesiredSize.Height);
            if (child is CanvasItem transformed)
                rectangle = ControlTransforms.TransformBounds(rectangle, transformed.GetVisualTransform(rectangle));
            width = MathF.Max(width, rectangle.Right);
            height = MathF.Max(height, rectangle.Bottom);
        }
        return new Size2(width, height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        foreach (Control child in Children)
        {
            Point2 position = child is CanvasItem item ? item.Position : default;
            child.Arrange(new Bounds2(contentBounds.X + position.X, contentBounds.Y + position.Y,
                child.DesiredSize.Width, child.DesiredSize.Height), true);
        }
    }

    /// <inheritdoc/>
    protected override void OnPreviewInput(ControlInputContext context, ControlInputEventArgs args)
    {
        base.OnPreviewInput(context, args);
        ControlInputEvent input = args.Input;
        if (_dragging && input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape)
        {
            FinishGesture(true);
            context.ReleasePointer(this, _pointer);
            args.Handled = true;
            return;
        }
        if (_dragging && input.Pointer == _pointer)
        {
            if (input.Type == ControlInputEventType.PointerMove)
            {
                if (!IsEditingEnabled || _selectedItem is not CanvasItem active || active.IsLocked || !ReferenceEquals(active.Parent, this))
                    FinishGesture(true);
                else
                    UpdateGesture(input);
                args.Handled = true;
                return;
            }
            if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
            {
                FinishGesture(false);
                context.ReleasePointer(this, _pointer);
                args.Handled = true;
                return;
            }
            if (input.Type == ControlInputEventType.PointerCancel)
            {
                FinishGesture(true);
                context.ReleasePointer(this, _pointer);
                args.Handled = true;
                return;
            }
        }
        if (!IsEditingEnabled || input.Type != ControlInputEventType.PointerDown || input.Button != MouseButton.Left || _dragging)
            return;

        Vector2 point = input.Position - new Vector2(Bounds.X, Bounds.Y);
        CanvasEditOperation operation = HitHandle(point, input.Alt, out int handle);
        if (operation == CanvasEditOperation.None)
        {
            CanvasItem? hit = null;
            for (Control? node = args.Source; node is not null && !ReferenceEquals(node, this); node = node.Parent)
                if (node is CanvasItem candidate && ReferenceEquals(candidate.Parent, this))
                    hit = candidate;
            SelectedItem = hit;
            operation = hit is { IsLocked: false, AllowMove: true } ? CanvasEditOperation.Move : CanvasEditOperation.None;
        }
        if (_selectedItem is not CanvasItem selected || selected.IsLocked || operation == CanvasEditOperation.None)
            return;

        _operation = operation;
        _handle = handle;
        _pointer = input.Pointer;
        _start = new Transform2State(selected);
        _startPointer = input.Position;
        _startInverse = Matrix.Invert(selected.VisualTransform);
        Vector2 pivot = PivotPosition(selected) + new Vector2(Bounds.X, Bounds.Y);
        _lastAngle = MathF.Atan2(input.Position.Y - pivot.Y, input.Position.X - pivot.X);
        _angle = 0;
        _dragging = true;
        context.Focus(this);
        context.CapturePointer(this);
        RaiseTransform(SelectionTransformState.Started);
        args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        FinishGesture(true);
        base.OnInteractionCancelled();
    }

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        if (_selectedItem is not null && !ReferenceEquals(_selectedItem.Parent, this))
            SelectedItem = null;
    }

    /// <summary>Gets a resize handle position after the item's visual transformation.</summary>
    /// <param name="item">The selected item.</param>
    /// <param name="index">The clockwise handle index, starting at the upper-left corner.</param>
    /// <returns>The handle position in canvas-local drawing coordinates.</returns>
    private Vector2 HandlePosition(CanvasItem item, int index)
    {
        float x = index is 0 or 6 or 7 ? 0 : index is 2 or 3 or 4 ? item.Bounds.Width : item.Bounds.Width / 2;
        float y = index is 0 or 1 or 2 ? 0 : index is 4 or 5 or 6 ? item.Bounds.Height : item.Bounds.Height / 2;
        Vector2 absolute = new(item.Bounds.X + x, item.Bounds.Y + y);
        return Vector2.Transform(absolute, item.VisualTransform) - new Vector2(Bounds.X, Bounds.Y);
    }

    /// <summary>Gets the rotation handle, offset outward by a constant canvas distance.</summary>
    /// <param name="item">The selected item.</param>
    /// <returns>The handle position in canvas coordinates.</returns>
    private Vector2 RotationHandle(CanvasItem item)
        => HandlePosition(item, 1) + new Vector2(MathF.Sin(item.Rotation), -MathF.Cos(item.Rotation)) * 24;

    /// <summary>Gets the visible pivot position.</summary>
    /// <param name="item">The selected item.</param>
    /// <returns>The pivot position in canvas coordinates.</returns>
    private Vector2 PivotPosition(CanvasItem item)
        => new(item.Bounds.X - Bounds.X + item.Bounds.Width * item.RotationPivot.X,
            item.Bounds.Y - Bounds.Y + item.Bounds.Height * item.RotationPivot.Y);

    /// <summary>Draws a handle with a filled center and contrasting outline.</summary>
    /// <param name="context">The current drawing context.</param>
    /// <param name="position">The canvas-local handle center.</param>
    /// <param name="color">The selection color.</param>
    private static void DrawHandle(ControlDrawContext context, Vector2 position, Color color)
    {
        Bounds2 bounds = new(position.X - 4, position.Y - 4, 8, 8);
        context.FillRectangle(bounds, new Color(20, 30, 40));
        context.StrokeRectangle(bounds, color, 1);
    }

    /// <summary>Finds a selected item's editor handle near the pointer.</summary>
    /// <param name="point">The pointer in canvas coordinates.</param>
    /// <param name="scale">Whether the Alt modifier requests scaling.</param>
    /// <param name="handle">Receives the resize handle index when applicable.</param>
    /// <returns>The available transformation operation.</returns>
    private CanvasEditOperation HitHandle(Vector2 point, bool scale, out int handle)
    {
        handle = -1;
        if (_selectedItem is not CanvasItem item || item.IsLocked || !item.IsVisible)
            return CanvasEditOperation.None;
        if (item.AllowPivot && Vector2.DistanceSquared(point, PivotPosition(item)) <= 49)
            return CanvasEditOperation.Pivot;
        if (item.AllowRotate && Vector2.DistanceSquared(point, RotationHandle(item)) <= 64)
            return CanvasEditOperation.Rotate;
        if (scale ? !item.AllowScale : !item.AllowResize)
            return CanvasEditOperation.None;
        for (int i = 0; i < 8; i++)
        {
            if (Vector2.DistanceSquared(point, HandlePosition(item, i)) > 64)
                continue;
            handle = i;
            return scale ? CanvasEditOperation.Scale : CanvasEditOperation.Resize;
        }
        return CanvasEditOperation.None;
    }

    /// <summary>Applies a captured pointer change relative to the original transform.</summary>
    /// <param name="input">The current pointer event in canvas layout coordinates.</param>
    private void UpdateGesture(ControlInputEvent input)
    {
        CanvasItem item = _selectedItem!;
        Transform2State start = _start!;
        Vector2 initialPosition = new(start.Position.X, start.Position.Y);
        Vector2 oldPivot = new(start.Size.Width * start.RotationPivot.X, start.Size.Height * start.RotationPivot.Y);
        Matrix linear = Matrix.CreateScale(start.Scale.X, start.Scale.Y, 1) * Matrix.CreateRotationZ(start.Rotation);
        if (_operation == CanvasEditOperation.Move)
        {
            Vector2 moved = initialPosition + input.Position - _startPointer;
            item.Position = new Point2(moved.X, moved.Y);
        }
        else if (_operation == CanvasEditOperation.Rotate)
        {
            Vector2 pivot = PivotPosition(item) + new Vector2(Bounds.X, Bounds.Y);
            float angle = MathF.Atan2(input.Position.Y - pivot.Y, input.Position.X - pivot.X);
            _angle += MathHelper.WrapAngle(angle - _lastAngle);
            _lastAngle = angle;
            float rotation = start.Rotation + _angle;
            item.Rotation = input.Shift ? MathF.Round(rotation / (MathF.PI / 12)) * (MathF.PI / 12) : rotation;
        }
        else if (_operation == CanvasEditOperation.Pivot)
        {
            Vector2 local = oldPivot + Vector2.Transform(input.Position, _startInverse)
                - Vector2.Transform(_startPointer, _startInverse);
            Vector2 delta = local - oldPivot;
            Vector2 position = initialPosition - delta + Vector2.TransformNormal(delta, linear);
            item.RotationPivot = new Point2(local.X / start.Size.Width, local.Y / start.Size.Height);
            item.Position = new Point2(position.X, position.Y);
        }
        else
        {
            Vector2 delta = Vector2.Transform(input.Position, _startInverse)
                - Vector2.Transform(_startPointer, _startInverse);
            bool left = _handle is 0 or 6 or 7;
            bool right = _handle is 2 or 3 or 4;
            bool top = _handle is 0 or 1 or 2;
            bool bottom = _handle is 4 or 5 or 6;
            float width = MathF.Max(1, start.Size.Width + (left ? -delta.X : right ? delta.X : 0));
            float height = MathF.Max(1, start.Size.Height + (top ? -delta.Y : bottom ? delta.Y : 0));
            if (input.Shift)
            {
                float xFactor = width / start.Size.Width;
                float yFactor = height / start.Size.Height;
                float factor = left || right ? top || bottom
                    ? MathF.Abs(xFactor - 1) > MathF.Abs(yFactor - 1) ? xFactor : yFactor
                    : xFactor : yFactor;
                width = start.Size.Width * factor;
                height = start.Size.Height * factor;
            }
            Vector2 anchor = new(left ? 1 : right ? 0 : .5f, top ? 1 : bottom ? 0 : .5f);
            Vector2 oldAnchor = new(start.Size.Width * anchor.X, start.Size.Height * anchor.Y);
            Vector2 fixedPosition = initialPosition + oldPivot + Vector2.TransformNormal(oldAnchor - oldPivot, linear);
            Vector2 newPivot;
            Vector2 newAnchor;
            Matrix newLinear;
            if (_operation == CanvasEditOperation.Scale)
            {
                item.Scale = new Vector2(MathF.Max(.001f, start.Scale.X * width / start.Size.Width),
                    MathF.Max(.001f, start.Scale.Y * height / start.Size.Height));
                newPivot = oldPivot;
                newAnchor = oldAnchor;
                newLinear = Matrix.CreateScale(item.Scale.X, item.Scale.Y, 1) * Matrix.CreateRotationZ(start.Rotation);
            }
            else
            {
                item.Size = new Size2(width, height);
                newPivot = new Vector2(width * start.RotationPivot.X, height * start.RotationPivot.Y);
                newAnchor = new Vector2(width * anchor.X, height * anchor.Y);
                newLinear = linear;
            }
            Vector2 position = fixedPosition - newPivot - Vector2.TransformNormal(newAnchor - newPivot, newLinear);
            item.Position = new Point2(position.X, position.Y);
        }
        RaiseTransform(SelectionTransformState.Changed);
    }

    /// <summary>Completes or restores the active transformation and releases its capture.</summary>
    /// <param name="cancel">Whether the original transform must be restored.</param>
    private void FinishGesture(bool cancel)
    {
        if (!_dragging || _selectedItem is not CanvasItem item || _start is not Transform2State start)
            return;
        _dragging = false;
        if (cancel)
        {
            item.Position = start.Position;
            item.Size = start.Size;
            item.Scale = start.Scale;
            item.Rotation = start.Rotation;
            item.RotationPivot = start.RotationPivot;
        }
        RaiseTransform(cancel ? SelectionTransformState.Cancelled : SelectionTransformState.Completed);
        _start = null;
        _operation = CanvasEditOperation.None;
        Workspace?.ReleasePointer(this, _pointer);
    }

    /// <summary>Raises a gesture lifecycle notification with its original transform.</summary>
    /// <param name="state">The current state of the active operation.</param>
    private void RaiseTransform(SelectionTransformState state)
    {
        if (_selectedItem is CanvasItem item && _start is Transform2State start)
            TransformChanged?.Invoke(this, new CanvasTransformEventArgs(item, _operation, state, start));
    }
}
