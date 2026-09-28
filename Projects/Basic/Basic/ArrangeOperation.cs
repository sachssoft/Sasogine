namespace Sachssoft.Engine;

/// <summary>
/// Defines operations for changing the ordering of an item within a collection or rendering order.
/// </summary>
public enum ArrangeOperation
{
    /// <summary>
    /// Moves the item one position forward.
    /// </summary>
    BringForward,

    /// <summary>
    /// Moves the item one position backward.
    /// </summary>
    SendBackward,

    /// <summary>
    /// Moves the item to the frontmost position.
    /// </summary>
    BringToFront,

    /// <summary>
    /// Moves the item to the backmost position.
    /// </summary>
    SendToBack
}