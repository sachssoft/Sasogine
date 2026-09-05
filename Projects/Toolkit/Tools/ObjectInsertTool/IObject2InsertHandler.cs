namespace Sachssoft.Sasogine.Components.Tools
{
    /// <summary>
    /// Defines a handler for creating, updating, completing,
    /// and canceling 2D object insertion operations.
    /// </summary>
    public interface IObject2InsertHandler
    {
        /// <summary>
        /// Creates an object when an insertion operation begins.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current 2D object insertion operation.
        /// </param>
        /// <returns>The created object.</returns>
        object Create(Object2InsertContext context);

        /// <summary>
        /// Updates the object while the insertion operation is being dragged.
        /// </summary>
        /// <param name="value">
        /// The object being inserted.
        /// </param>
        /// <param name="context">
        /// Provides information about the current 2D object insertion operation.
        /// </param>
        void Drag(
            object value,
            Object2InsertContext context);

        /// <summary>
        /// Completes the insertion operation.
        /// </summary>
        /// <param name="value">
        /// The object being inserted.
        /// </param>
        /// <param name="context">
        /// Provides information about the completed 2D object insertion operation.
        /// </param>
        void Complete(
            object value,
            Object2InsertContext context);

        /// <summary>
        /// Cancels the insertion operation.
        /// </summary>
        /// <param name="value">
        /// The object being inserted.
        /// </param>
        /// <param name="context">
        /// Provides information about the current 2D object insertion operation.
        /// </param>
        void Cancel(
            object value,
            Object2InsertContext context);
    }
}