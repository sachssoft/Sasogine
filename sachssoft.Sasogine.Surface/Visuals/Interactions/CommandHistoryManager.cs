using System;
using System.Collections.Generic;
using System.Linq;

namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

public class CommandHistoryManager
{
    private readonly Stack<IRestorableCommandCore> _undo_stack;
    private readonly Stack<IRestorableCommandCore> _redo_stack;
    private readonly int _size;

    public event EventHandler? UndoStackChanged;
    public event EventHandler? RedoStackChanged;

    public CommandHistoryManager(int size)
    {
        _undo_stack = new Stack<IRestorableCommandCore>();
        _redo_stack = new Stack<IRestorableCommandCore>();
        _size = size;
    }

    public CommandHistoryManager() : this(0)
    {        
    }

    public void Do(IRestorableCommandCore command, params object[] args)
    {
        command.Execute(args);
        _undo_stack.Push(command);

        if (_size > 0 && _undo_stack.Count > _size)
        {
            // Ältestes Element entfernen:
            var temp = _undo_stack.Reverse().Skip(1).Reverse().ToList();
            _undo_stack.Clear();
            foreach (var cmd in temp)
                _undo_stack.Push(cmd);
        }

        _redo_stack.Clear();

        OnUndoRedoChanged();
    }

    public void Undo(params object[] args)
    {
        if (_undo_stack.Count == 0)
            return;

        var restorable = _undo_stack.Pop();

        restorable.Unexecute(args);
        _redo_stack.Push(restorable);
        OnUndoRedoChanged();
    }

    public void Redo(params object[] args)
    {
        if (_redo_stack.Count == 0)
            return;

        var command = _redo_stack.Pop();

        command.Execute(args);
        _undo_stack.Push(command);
        OnUndoRedoChanged();
    }

    public bool CanUndo => _undo_stack.Count > 0;
    public bool CanRedo => _redo_stack.Count > 0;

    public void Clear()
    {
        _undo_stack.Clear();
        _redo_stack.Clear();
        OnUndoRedoChanged();
    }

    // Hilfsmethoden für UI (z.B. Befehlsbeschriftungen)
    //public IEnumerable<string> UndoLabels => _undo_stack.Select(c => GetCommandLabel(c));
    //public IEnumerable<string> RedoLabels => _redo_stack.Select(c => GetCommandLabel(c));

    //private string GetCommandLabel(ICommandCore command)
    //{
    //    var prop = command.GetType().GetProperty("Label");
    //    if (prop != null)
    //    {
    //        var val = prop.GetValue(command);
    //        if (val != null)
    //            return val.ToString() ?? string.Empty;
    //    }
    //    return command.GetType().Name;
    //}

    private void OnUndoRedoChanged()
    {
        UndoStackChanged?.Invoke(this, EventArgs.Empty);
        RedoStackChanged?.Invoke(this, EventArgs.Empty);
    }
}
