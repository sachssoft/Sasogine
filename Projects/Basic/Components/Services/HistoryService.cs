
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Services;

/// <summary>
/// Provides undo and redo history management with save-point tracking
/// and document modification notifications.
/// </summary>
public sealed class HistoryService : IComponentService
{
    private readonly Stack<HistoryEntry> _undoStack = new();
    private readonly Stack<HistoryEntry> _redoStack = new();
    private readonly int _maximumHistory;

    private long _nextStateId;
    private long _currentStateId;
    private long _savedStateId;
    private bool _forcedModified;

    /// <summary>
    /// Occurs when the availability of undo operations changes.
    /// </summary>
    public event EventHandler? CanUndoChanged;

    /// <summary>
    /// Occurs when the availability of redo operations changes.
    /// </summary>
    public event EventHandler? CanRedoChanged;

    /// <summary>
    /// Occurs whenever the undo or redo history changes.
    /// </summary>
    public event EventHandler? HistoryChanged;

    /// <summary>
    /// Occurs when the document modification state changes.
    /// </summary>
    public event EventHandler? IsModifiedChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryService"/> class.
    /// </summary>
    /// <param name="maximumHistory">The maximum number of undo actions retained.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maximumHistory"/> is not positive.
    /// </exception>
    public HistoryService(int maximumHistory = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumHistory);
        _maximumHistory = maximumHistory;
    }

    /// <summary>
    /// Gets whether an undo operation is available.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets whether a redo operation is available.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Gets whether the document contains unsaved changes.
    /// </summary>
    public bool IsModified => _forcedModified || _currentStateId != _savedStateId;

    /// <summary>
    /// Gets the maximum number of undo actions retained.
    /// </summary>
    public int MaximumHistory => _maximumHistory;

    /// <summary>
    /// Gets the number of available undo actions.
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Gets the number of available redo actions.
    /// </summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// Gets the available undo actions, ordered from newest to oldest.
    /// </summary>
    public IEnumerable<HistoryAction> UndoHistories
    {
        get
        {
            foreach (var entry in _undoStack)
                yield return entry.Action;
        }
    }

    /// <summary>
    /// Gets the available redo actions, ordered from next to last.
    /// </summary>
    public IEnumerable<HistoryAction> RedoHistories
    {
        get
        {
            foreach (var entry in _redoStack)
                yield return entry.Action;
        }
    }

    /// <summary>
    /// Gets the next action to undo, or null if none is available.
    /// </summary>
    public HistoryAction? NextUndo =>
        _undoStack.TryPeek(out var entry) ? entry.Action : null;

    /// <summary>
    /// Gets the next action to redo, or null if none is available.
    /// </summary>
    public HistoryAction? NextRedo =>
        _redoStack.TryPeek(out var entry) ? entry.Action : null;

    /// <summary>
    /// Executes an action and adds it to the undo history.
    /// </summary>
    /// <param name="action">The action to execute and record.</param>
    public void Execute(HistoryAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        bool oldUndo = CanUndo;
        bool oldRedo = CanRedo;
        bool oldModified = IsModified;

        action.Execute();

        long nextStateId = ++_nextStateId;
        _undoStack.Push(new HistoryEntry(action, _currentStateId, nextStateId));

        _currentStateId = nextStateId;
        _redoStack.Clear();

        TrimHistory();
        RaiseChanged(oldUndo, oldRedo, oldModified);
    }

    /// <summary>
    /// Undoes the most recently executed action.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
            return;

        bool oldUndo = CanUndo;
        bool oldRedo = CanRedo;
        bool oldModified = IsModified;

        var entry = _undoStack.Peek();
        entry.Action.Undo();

        _undoStack.Pop();
        _redoStack.Push(entry);
        _currentStateId = entry.BeforeStateId;

        RaiseChanged(oldUndo, oldRedo, oldModified);
    }

    /// <summary>
    /// Reexecutes the most recently undone action.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
            return;

        bool oldUndo = CanUndo;
        bool oldRedo = CanRedo;
        bool oldModified = IsModified;

        var entry = _redoStack.Peek();
        entry.Action.Execute();

        _redoStack.Pop();
        _undoStack.Push(entry);
        _currentStateId = entry.AfterStateId;

        RaiseChanged(oldUndo, oldRedo, oldModified);
    }

    /// <summary>
    /// Marks the current document state as successfully saved.
    /// </summary>
    /// <remarks>
    /// Call this method only after the document has been saved successfully.
    /// </remarks>
    public void MarkSaved()
    {
        bool oldModified = IsModified;

        _savedStateId = _currentStateId;
        _forcedModified = false;

        RaiseModifiedChanged(oldModified);
    }

    /// <summary>
    /// Explicitly marks the document as modified without adding a history action.
    /// </summary>
    /// <remarks>
    /// Use this method for changes that cannot be represented by a history action.
    /// Such changes remain marked as modified until the next successful save.
    /// </remarks>
    public void MarkModified()
    {
        bool oldModified = IsModified;
        _forcedModified = true;

        RaiseModifiedChanged(oldModified);
    }

    /// <summary>
    /// Clears the undo and redo history while preserving the document
    /// modification state.
    /// </summary>
    public void Clear()
    {
        bool oldUndo = CanUndo;
        bool oldRedo = CanRedo;
        bool oldModified = IsModified;

        bool wasModified = IsModified;

        _undoStack.Clear();
        _redoStack.Clear();

        _currentStateId = ++_nextStateId;
        _savedStateId = _currentStateId;
        _forcedModified = wasModified;

        RaiseChanged(oldUndo, oldRedo, oldModified);
    }

    /// <summary>
    /// Clears the history and establishes a clean baseline for a newly
    /// created or successfully loaded document.
    /// </summary>
    public void Reset()
    {
        bool oldUndo = CanUndo;
        bool oldRedo = CanRedo;
        bool oldModified = IsModified;

        _undoStack.Clear();
        _redoStack.Clear();

        _currentStateId = ++_nextStateId;
        _savedStateId = _currentStateId;
        _forcedModified = false;

        RaiseChanged(oldUndo, oldRedo, oldModified);
    }

    private void RaiseChanged(bool oldUndo, bool oldRedo, bool oldModified)
    {
        if (oldUndo != CanUndo)
            CanUndoChanged?.Invoke(this, EventArgs.Empty);

        if (oldRedo != CanRedo)
            CanRedoChanged?.Invoke(this, EventArgs.Empty);

        HistoryChanged?.Invoke(this, EventArgs.Empty);
        RaiseModifiedChanged(oldModified);
    }

    private void RaiseModifiedChanged(bool oldModified)
    {
        if (oldModified != IsModified)
            IsModifiedChanged?.Invoke(this, EventArgs.Empty);
    }

    private void TrimHistory()
    {
        if (_undoStack.Count <= _maximumHistory)
            return;

        var entries = _undoStack.ToArray();
        _undoStack.Clear();

        for (int i = _maximumHistory - 1; i >= 0; i--)
            _undoStack.Push(entries[i]);
    }

    private sealed record HistoryEntry(
        HistoryAction Action,
        long BeforeStateId,
        long AfterStateId);
}
