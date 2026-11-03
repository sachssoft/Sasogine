namespace Sachssoft.Engine;

/// <summary>
/// Specifies how an element occupies an available layout slot.
/// </summary>
public enum LayoutAlignment
{
    /// <summary>
    /// Aligns the element with the start of the slot.
    /// </summary>
    Start = 0,

    /// <summary>
    /// Centers the element within the slot.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Aligns the element with the end of the slot.
    /// </summary>
    End = 2,

    /// <summary>
    /// Uses the available space, subject to explicit size constraints.
    /// </summary>
    Stretch = 3
}
