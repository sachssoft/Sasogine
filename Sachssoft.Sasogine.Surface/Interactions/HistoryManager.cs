using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public sealed class HistoryManager : INotifyPropertyChanged
    {
        private readonly Stack<(Action Undo, Action Redo)> _undoStack;
        private readonly Stack<(Action Undo, Action Redo)> _redoStack;
        private readonly int _size;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? UndoStackChanged;
        public event EventHandler? RedoStackChanged;

        public HistoryManager(int size = 0)
        {
            _undoStack = new Stack<(Action Undo, Action Redo)>(size > 0 ? size : 16);
            _redoStack = new Stack<(Action Undo, Action Redo)>(size > 0 ? size : 16);
            _size = size;
        }

        // ---------------------
        // Do / Undo / Redo
        // ---------------------

        public void Do(Action @do, Action undo, params object[] args)
        {
            if (@do == null) throw new ArgumentNullException(nameof(@do));
            if (undo == null) throw new ArgumentNullException(nameof(undo));

            @do.Invoke();

            _undoStack.Push((Undo: undo, Redo: () => @do.Invoke()));

            TrimIfNecessary();
            _redoStack.Clear();

            NotifyStateChanged();
        }

        public void Undo(params object[] args)
        {
            if (_undoStack.Count == 0)
                return;

            var item = _undoStack.Pop();

            item.Undo.Invoke();
            _redoStack.Push(item);

            NotifyStateChanged();
        }

        public void Redo(params object[] args)
        {
            if (_redoStack.Count == 0)
                return;

            var item = _redoStack.Pop();

            item.Redo.Invoke();
            _undoStack.Push(item);

            NotifyStateChanged();
        }

        // ---------------------
        // Helper
        // ---------------------

        private void TrimIfNecessary()
        {
            if (_size <= 0) return;

            while (_undoStack.Count > _size)
            {
                // Entfernt das älteste Element – effizient mit Hilfsstack
                var temp = new Stack<(Action Undo, Action Redo)>(_size);

                while (temp.Count < _size)
                    temp.Push(_undoStack.Pop());

                _undoStack.Clear();

                foreach (var item in temp)
                    _undoStack.Push(item);
            }
        }

        private void NotifyStateChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanUndo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRedo)));

            UndoStackChanged?.Invoke(this, EventArgs.Empty);
            RedoStackChanged?.Invoke(this, EventArgs.Empty);
        }

        // ---------------------
        // Properties
        // ---------------------

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();

            NotifyStateChanged();
        }
    }
}
