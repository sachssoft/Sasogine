using Sachssoft.Sasogine.Interactions;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public class CommandRule<TSender> : ICommandRule
    {
        private readonly Func<TSender, object?[]?, bool> _canExecute;
        private readonly List<ICommand> _commands = new();

        public CommandRule(Func<TSender, object?[]?, bool> canExecute)
        {
            _canExecute = canExecute
                ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public bool Evaluate(TSender sender, object?[]? args)
        {
            return _canExecute.Invoke(sender, args);
        }

        public void Bind(ICommand command)
        {
            if (!_commands.Contains(command))
                _commands.Add(command);
        }

        public void NotifyCanExecuteChanged()
        {
            foreach (var cmd in _commands)
            {
                if (cmd is IRaiseCanExecuteChangedCommand custom)
                    custom.RaiseCanExecuteChanged();
            }
        }

        // Non-generic Interface Implementierung
        bool ICommandRule.Evaluate(object? sender, object?[]? args)
        {
            if (sender is not TSender s)
                return true;

            return Evaluate(s, args);
        }

    }
}
