using Sachssoft.Sasogine.Surface.Interactions;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Interactions
{
    /// <summary>
    /// Ein generischer ICommand-Wrapper, der Actions mit optionaler CanExecute-Prüfung unterstützt.
    /// Unterstützt auch ICommandCondition und Lazy-Bind.
    /// </summary>
    public class BoundCommand<TSender> : IRaiseCanExecuteChangedCommand
    {
        private readonly Action<TSender?, object?[]?> _execute;
        private readonly Func<TSender?, object?[]?, bool>? _canExecute;
        private readonly Func<TSender, ICommandRule>? _conditionLazy;
        private ICommandRule? _conditionCached;

        public event EventHandler? CanExecuteChanged;

        // ---------------------------------------
        // Konstruktor: Standard
        // ---------------------------------------
        public BoundCommand(
            Action<TSender?, object?[]?> execute,
            Func<TSender?, object?[]?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public BoundCommand(
            Action<TSender?, object?[]?> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public BoundCommand(
            ICommandRule condition,
            Action<TSender?, object?[]?> execute)
        {
            _conditionCached = condition;
            _conditionCached.Bind(this);
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public BoundCommand(
            Func<TSender, ICommandRule> conditionLazy,
            Action<TSender?, object?[]?> execute)
        {
            _conditionLazy = conditionLazy;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        // ---------------------------------------
        // ActionCommand API
        // ---------------------------------------
        public bool CanExecute(TSender? sender, params object?[]? args)
        {
            if (sender is null)
                return true;

            // Lazy Binding nur einmal ausführen
            if (_conditionCached == null && _conditionLazy != null)
            {
                _conditionCached = _conditionLazy(sender);
                _conditionCached.Bind(this);
            }

            if (_conditionCached != null)
            {
                return _conditionCached.Evaluate(sender, args);
            }

            return _canExecute?.Invoke(sender, args) ?? true;
        }

        public void Execute(TSender? sender, params object?[]? args)
        {
            if (CanExecute(sender, args))
                _execute(sender, args ?? Array.Empty<object?>());
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        // ---------------------------------------
        // ICommand-Bridge
        // Parameter MUST be: new object[] { sender, arg1, arg2, ... }
        // ---------------------------------------
        bool ICommand.CanExecute(object? parameter)
        {
            EnsureIsSenderValid(parameter, out var sender, out var args);
            return CanExecute(sender, args);
        }

        void ICommand.Execute(object? parameter)
        {
            EnsureIsSenderValid(parameter, out var sender, out var args);
            Execute(sender, args);
        }

        private void EnsureIsSenderValid(object? parameter, out TSender? sender, out object?[]? args)
        {
            if (parameter is not object?[] arr || arr.Length == 0)
                throw new ArgumentException(
                    "Parameter must be an object[] with sender as the first element.");

            sender = default;
            if (arr[0] is TSender s)
                sender = s;

            args = arr.Length > 1 ? arr[1..] : Array.Empty<object?>();
        }
    }
}
