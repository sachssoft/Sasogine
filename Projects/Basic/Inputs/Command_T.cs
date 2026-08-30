using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Represents an executable command with a typed parameter.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class Command<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The function that determines whether the command can execute.</param>
        public Command(
            Action<T?> execute,
            Func<T?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when the result of <see cref="CanExecute"/> may have changed.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Gets whether the command can currently be executed.
        /// </summary>
        public virtual bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// Executes the command if it can currently be executed.
        /// </summary>
        public virtual void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute((T?)parameter);
        }

        /// <summary>
        /// Notifies listeners that <see cref="CanExecute"/> may have changed.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}