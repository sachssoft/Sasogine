namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Defines a handler for creating, updating, completing,
    /// and canceling object insertion operations.
    /// </summary>
    public interface IObjectInsertHandler
    {
        /// <summary>
        /// Creates an object when an insertion operation begins.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current insertion operation.
        /// </param>
        /// <returns>The created object.</returns>
        object Create(ObjectInsertContext context);

        /// <summary>
        /// Updates the object while the insertion operation is being dragged.
        /// </summary>
        /// <param name="value">The object being inserted.</param>
        /// <param name="context">
        /// Provides information about the current insertion operation.
        /// </param>
        void Drag(
            object value,
            ObjectInsertContext context);

        /// <summary>
        /// Completes the insertion operation.
        /// </summary>
        /// <param name="value">The object being inserted.</param>
        /// <param name="context">
        /// Provides information about the completed insertion operation.
        /// </param>
        void Complete(
            object value,
            ObjectInsertContext context);

        /// <summary>
        /// Cancels the insertion operation.
        /// </summary>
        /// <param name="value">The object being inserted.</param>
        /// <param name="context">
        /// Provides information about the current insertion operation.
        /// </param>
        void Cancel(
            object value,
            ObjectInsertContext context);
    }
}