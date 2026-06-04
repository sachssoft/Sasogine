using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Inputs;

/// <summary>
/// Schneller ActionCommand für Hot-Loops, GC-frei
/// </summary>
public readonly struct ActionCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly EventHandler? _canExecuteChangedHandlers;

    public ActionCommand(
        Action<object?> execute,
        Func<object?, bool>? canExecute = null,
        EventHandler? canExecuteChangedHandlers = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _canExecuteChangedHandlers = canExecuteChangedHandlers;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    event EventHandler? ICommand.CanExecuteChanged
    {
        add { }  // UI-Handler wird extern übergeben
        remove { }
    }

    public void RaiseCanExecuteChanged()
    {
        _canExecuteChangedHandlers?.Invoke(this, EventArgs.Empty);
    }
}