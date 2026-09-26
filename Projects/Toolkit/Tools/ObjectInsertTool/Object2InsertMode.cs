namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines how a 2D object is inserted.
/// </summary>
public enum Object2InsertMode
{
    /// <summary>
    /// Inserts the object immediately without dragging.
    /// </summary>
    Fixed,

    /// <summary>
    /// Inserts the object by dragging with equal width and height.
    /// </summary>
    Square,

    /// <summary>
    /// Inserts the object by dragging with unrestricted width and height.
    /// </summary>
    Free
}
