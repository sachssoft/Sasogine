using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Represents control layout context.
/// </summary>
public sealed class ControlLayoutContext
{
    /// <summary>
    /// Initializes a context with the default font used for measuring and drawing.
    /// </summary>
    public ControlLayoutContext(IControlFont defaultFont)
    {
        DefaultFont = defaultFont ?? throw new ArgumentNullException(nameof(defaultFont));
    }

    /// <summary>
    /// Gets the default font. The caller owns its lifetime.
    /// </summary>
    public IControlFont DefaultFont { get; }
}
