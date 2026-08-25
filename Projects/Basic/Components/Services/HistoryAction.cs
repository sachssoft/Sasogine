using System;

namespace Sachssoft.Sasogine.Components.Services
{
    /// <summary>
    /// Represents a reversible editor operation.
    /// </summary>
    public class HistoryAction
    {
        private readonly Action _execute;
        private readonly Action _undo;


        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryAction"/> class.
        /// </summary>
        /// <param name="execute">
        /// Action that applies the change.
        /// </param>
        /// <param name="undo">
        /// Action that reverts the change.
        /// </param>
        /// <param name="label">
        /// Optional display label of the history action.
        /// </param>
        public HistoryAction(
            Action execute,
            Action undo,
            string? label = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _undo = undo ?? throw new ArgumentNullException(nameof(undo));

            Label = label ?? string.Empty;
        }


        /// <summary>
        /// Gets the display label of this history action.
        /// </summary>
        public string Label { get; }


        /// <summary>
        /// Executes the operation.
        /// </summary>
        public void Execute()
        {
            _execute();
        }


        /// <summary>
        /// Reverts the operation.
        /// </summary>
        public void Undo()
        {
            _undo();
        }
    }
}