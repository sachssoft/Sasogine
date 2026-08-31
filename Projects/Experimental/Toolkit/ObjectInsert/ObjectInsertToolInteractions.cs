using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines the input interactions used by the <see cref="ObjectInsertTool"/>.
/// </summary>
public sealed class ObjectInsertToolInteractions
{
    /// <summary>
    /// Gets or sets the primary interaction used to insert an object.
    /// </summary>
    public InteractionFlags Action { get; set; } = InteractionFlags.None;

    /// <summary>
    /// Gets or sets the interaction used to cancel the current insertion.
    /// </summary>
    public InteractionFlags Cancel { get; set; } = InteractionFlags.None;
}