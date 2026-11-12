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
    /// <param name="application">The application that owns this workspace's services.</param>
    /// <param name="input">The input snapshot for this update.</param>
    /// <param name="layout">The optional layout context. The workspace resolves the standard font by default.</param>
    public ControlUpdateContext(GameApplicationBase application, ControlInputFrame input, ControlLayoutContext? layout = null) : base(application)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Layout = layout ?? ControlLayoutContext.Default;
        FontBackend = application.GetService<IFontBackend>();
    }

    internal IFontBackend? FontBackend { get; }

    /// <summary>
    /// Gets the immutable input and elapsed-time snapshot.
    /// </summary>
    public ControlInputFrame Input { get; }
    /// <summary>
    /// Gets layout services.
    /// </summary>
    public ControlLayoutContext Layout { get; }
}
