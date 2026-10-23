using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Controls.Input;
/// <summary>
/// Represents control input frame.
/// </summary>
public sealed class ControlInputFrame
{
    /// <summary>
    /// Copies events and validates the supplied logical size and elapsed time.
    /// </summary>
    public ControlInputFrame(Size2 size, TimeSpan elapsed, IEnumerable<ControlInputEvent>? events = null)
    {
        Control.ValidateSize(size, false);
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        Size = size;
        Elapsed = elapsed;
        var items = events is null ? new List<ControlInputEvent>() : new List<ControlInputEvent>(events);
        foreach (ControlInputEvent input in items)
        {
            if (input.Type is not (ControlInputEventType.PointerMove or ControlInputEventType.PointerDown or ControlInputEventType.PointerUp or ControlInputEventType.PointerWheel or ControlInputEventType.KeyDown or ControlInputEventType.KeyUp or ControlInputEventType.TextInput or ControlInputEventType.Deactivate or ControlInputEventType.PointerCancel) || input.PointerKind is not (ControlPointerKind.Mouse or ControlPointerKind.Touch) || (input.PointerKind == ControlPointerKind.Mouse && input.PointerId != 0) || input.Button is not (Sachssoft.Engine.Input.MouseButton.Left or Sachssoft.Engine.Input.MouseButton.Right or Sachssoft.Engine.Input.MouseButton.Middle or Sachssoft.Engine.Input.MouseButton.XButton1 or Sachssoft.Engine.Input.MouseButton.XButton2) || !float.IsFinite(input.Position.X) || !float.IsFinite(input.Position.Y))
                throw new ArgumentException("Invalid input event.", nameof(events));
        }

        Events = new ReadOnlyCollection<ControlInputEvent>(items);
    }

    /// <summary>
    /// Gets the physical viewport when created by ControlInputSampler; null denotes an explicitly logical frame.
    /// </summary>
    public Microsoft.Xna.Framework.Rectangle? PhysicalViewport { get; private set; }
    /// <summary>
    /// Gets the sampler's pixels-per-logical-unit transform, retained for deterministic coordinate conversion.
    /// </summary>
    public float SourceScale { get; private set; } = 1;
    internal ControlInputFrame SetViewport(Microsoft.Xna.Framework.Rectangle viewport, float scale)
    {
        PhysicalViewport = viewport; SourceScale = scale; return this;
    }

    /// <summary>
    /// Gets the logical viewport size.
    /// </summary>
    public Size2 Size { get; private set; }
    /// <summary>
    /// Gets the caller-supplied elapsed time.
    /// </summary>
    public TimeSpan Elapsed { get; private set; }
    /// <summary>
    /// Gets events in the exact order in which they will be processed.
    /// </summary>
    public IReadOnlyList<ControlInputEvent> Events { get; }

    internal ControlInputFrame(List<ControlInputEvent> borrowed)
    {
        Events = borrowed.AsReadOnly();
    }

    internal ControlInputFrame Reset(Size2 size, TimeSpan elapsed)
    {
        Control.ValidateSize(size, false);
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        for (int i = 0; i < Events.Count; i++)
            if (!float.IsFinite(Events[i].Position.X) || !float.IsFinite(Events[i].Position.Y))
                throw new ArgumentException("Input position must be finite.");
        Size = size;
        Elapsed = elapsed;
        return this;
    }
}
