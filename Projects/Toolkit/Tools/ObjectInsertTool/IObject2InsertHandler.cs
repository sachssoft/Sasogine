namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines a handler for creating, updating, completing,
/// and canceling 2D definition insertion operations.
/// </summary>
public interface IObject2InsertHandler
{
    /// <summary>
    /// Creates a definition when an insertion operation begins.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current 2D insertion operation.
    /// </param>
    /// <returns>The created definition.</returns>
    IDefinition Create(Object2InsertContext context);

    /// <summary>
    /// Updates the definition while the insertion operation is being dragged.
    /// </summary>
    /// <param name="definition">
    /// The definition being inserted.
    /// </param>
    /// <param name="context">
    /// Provides information about the current 2D insertion operation.
    /// </param>
    void Drag(
        IDefinition definition,
        Object2InsertContext context);

    /// <summary>
    /// Completes the insertion operation.
    /// </summary>
    /// <param name="definition">
    /// The definition being inserted.
    /// </param>
    /// <param name="context">
    /// Provides information about the completed 2D insertion operation.
    /// </param>
    void Complete(
        IDefinition definition,
        Object2InsertContext context);

    /// <summary>
    /// Cancels the insertion operation.
    /// </summary>
    /// <param name="definition">
    /// The definition being inserted.
    /// </param>
    /// <param name="context">
    /// Provides information about the current 2D insertion operation.
    /// </param>
    void Cancel(
        IDefinition definition,
        Object2InsertContext context);
}