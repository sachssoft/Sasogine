using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Interactions;

public class ActionCommand : ICommand
{
    private readonly Action<SceneBase, object[]> _execute;
    private readonly Func<SceneBase, object[], bool>? _can_execute;

    public event EventHandler? CanExecuteChanged;

    public ActionCommand(Action<SceneBase, object[]> execute, Func<SceneBase, object[], bool>? can_execute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _can_execute = can_execute;
    }

    public bool CanExecute(SceneBase view, params object[] args)
        => _can_execute?.Invoke(view, args) ?? true;

    public void Execute(SceneBase view, params object[] args)
    {
        if (CanExecute(view, args))
            _execute(view, args);
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}