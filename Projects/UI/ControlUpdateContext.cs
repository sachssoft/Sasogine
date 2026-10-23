using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Adapts a deterministic UI frame to Sasogine's application context model.
/// </summary>
public sealed class ControlUpdateContext : GameContext
{
    /// <summary>
    /// Initializes a UI update context. SetFrameTime is independent of the immutable input frame.
    /// </summary>
    public ControlUpdateContext(GameApplicationBase application, ControlInputFrame input, ControlLayoutContext layout) : base(application)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
    }

    /// <summary>
    /// Gets the immutable input and elapsed-time snapshot.
    /// </summary>
    public ControlInputFrame Input { get; }
    /// <summary>
    /// Gets layout services.
    /// </summary>
    public ControlLayoutContext Layout { get; }
}
