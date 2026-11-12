using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Represents control layout context.
/// </summary>
public sealed class ControlLayoutContext
{
    /// <summary>Gets the shared layout context for the standard built-in UI font.</summary>
    public static ControlLayoutContext Default => SharedDefault.Value;

    private static readonly Lazy<ControlLayoutContext> SharedDefault = new(static () => new());

    /// <summary>
    /// Initializes a context with the default font used for measuring and drawing.
    /// </summary>
    /// <param name="defaultFont">The optional caller-owned font. Defaults to Noto Sans Regular.</param>
    public ControlLayoutContext(IFont? defaultFont = null)
    {
        UsesDefaultFont = defaultFont is null;
        DefaultFont = defaultFont ?? BuiltInFonts.GetFont(BuiltInFont.Sans, BuiltInFonts.DefaultSize);
    }

    internal bool UsesDefaultFont { get; }

    /// <summary>
    /// Gets the default font. The caller owns its lifetime.
    /// </summary>
    public IFont DefaultFont { get; }
}
