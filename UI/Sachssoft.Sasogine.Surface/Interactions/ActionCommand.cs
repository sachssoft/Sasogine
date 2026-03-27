using Sachssoft.Sasogine.Interactions;
using System;

public class ActionCommand : IRaiseCanExecuteChangedCommand
{
    private readonly Action<object?[]?> _execute;
    private readonly Func<object?[]?, bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    public ActionCommand(Action<object?[]?> execute, Func<object?[]?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter as object?[]) ?? true;

    public void Execute(object? parameter) => _execute(parameter as object?[] ?? Array.Empty<object?>());

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
