using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Services
{
    /// <summary>
    /// Provides undo and redo history management.
    /// </summary>
    public sealed class HistoryService
    {
        private readonly Stack<HistoryAction> _undoStack = new();
        private readonly Stack<HistoryAction> _redoStack = new();

        private readonly int _maximumHistory;


        /// <summary>
        /// Occurs when the undo availability changes.
        /// </summary>
        public event EventHandler? CanUndoChanged;


        /// <summary>
        /// Occurs when the redo availability changes.
        /// </summary>
        public event EventHandler? CanRedoChanged;


        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryService"/> class.
        /// </summary>
        /// <param name="maximumHistory">
        /// Maximum number of stored undo actions.
        /// </param>
        public HistoryService(int maximumHistory = 100)
        {
            if (maximumHistory <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumHistory));

            _maximumHistory = maximumHistory;
        }


        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;


        /// <summary>
        /// Gets a value indicating whether a redo operation is available.
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;


        /// <summary>
        /// Executes and stores a history action.
        /// </summary>
        public void Execute(HistoryAction action)
        {
            ArgumentNullException.ThrowIfNull(action);

            bool oldUndo = CanUndo;
            bool oldRedo = CanRedo;

            action.Execute();

            _undoStack.Push(action);
            _redoStack.Clear();

            TrimHistory();

            RaiseChanged(oldUndo, oldRedo);
        }


        /// <summary>
        /// Undoes the last history action.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;

            bool oldUndo = CanUndo;
            bool oldRedo = CanRedo;

            var action = _undoStack.Pop();

            action.Undo();

            _redoStack.Push(action);

            RaiseChanged(oldUndo, oldRedo);
        }


        /// <summary>
        /// Redoes the last undone history action.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;

            bool oldUndo = CanUndo;
            bool oldRedo = CanRedo;

            var action = _redoStack.Pop();

            action.Execute();

            _undoStack.Push(action);

            RaiseChanged(oldUndo, oldRedo);
        }


        /// <summary>
        /// Clears all history actions.
        /// </summary>
        public void Clear()
        {
            bool oldUndo = CanUndo;
            bool oldRedo = CanRedo;

            _undoStack.Clear();
            _redoStack.Clear();

            RaiseChanged(oldUndo, oldRedo);
        }


        private void RaiseChanged(bool oldUndo, bool oldRedo)
        {
            if (oldUndo != CanUndo)
                CanUndoChanged?.Invoke(this, EventArgs.Empty);

            if (oldRedo != CanRedo)
                CanRedoChanged?.Invoke(this, EventArgs.Empty);
        }


        private void TrimHistory()
        {
            if (_undoStack.Count <= _maximumHistory)
                return;

            var actions = _undoStack.ToArray();

            _undoStack.Clear();

            for (int i = 0; i < _maximumHistory; i++)
            {
                _undoStack.Push(actions[i]);
            }
        }
    }
}