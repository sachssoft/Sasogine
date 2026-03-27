using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Inputs
{
    /// <summary>
    /// DelayedCommand für verzögerte Ausführung, GC-frei bei Structs, ICommand-kompatibel
    /// </summary>
    public readonly struct DelayedCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly int _delayMs;
        private readonly Func<object?, bool>? _canExecute;
        private readonly EventHandler? _canExecuteChangedHandlers;

        public DelayedCommand(
            Action<object?> execute,
            int delayMs,
            Func<object?, bool>? canExecute = null,
            EventHandler? canExecuteChangedHandlers = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _delayMs = delayMs;
            _canExecute = canExecute;
            _canExecuteChangedHandlers = canExecuteChangedHandlers;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            if (_delayMs > 0)
                await Task.Delay(_delayMs).ConfigureAwait(false);
            _execute(parameter);
        }

        event EventHandler? ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void RaiseCanExecuteChanged()
        {
            _canExecuteChangedHandlers?.Invoke(this, EventArgs.Empty);
        }
    }
}