using sachssoft.Sasogine.Surface;
using System;

namespace sachssoft.Sasogine.Interactions;

public class ActionCommand : ICommand
{
    private readonly Action<ViewBase, object[]> _execute;
    private readonly Func<ViewBase, object[], bool>? _can_execute;

    public event EventHandler? CanExecuteChanged;

    public ActionCommand(Action<ViewBase, object[]> execute, Func<ViewBase, object[], bool>? can_execute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _can_execute = can_execute;
    }

    public bool CanExecute(ViewBase view, params object[] args)
        => _can_execute?.Invoke(view, args) ?? true;

    public void Execute(ViewBase view, params object[] args)
    {
        if (CanExecute(view, args))
            _execute(view, args);
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}