using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Interactions;

public class ActionCommand<TView> : ICommand
    where TView : SceneBase
{
    private readonly Action<TView, object[]> _execute;
    private readonly Func<TView, object[], bool>? _can_execute;

    public event EventHandler? CanExecuteChanged;

    public ActionCommand(Action<TView, object[]> execute, Func<TView, object[], bool>? can_execute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _can_execute = can_execute;
    }

    public bool CanExecute(TView view, params object[] args)
        => _can_execute?.Invoke(view, args) ?? true;

    public void Execute(TView view, params object[] args)
    {
        if (CanExecute(view, args))
            _execute(view, args);
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    bool ICommand.CanExecute(SceneBase view, params object[] args)
        => CanExecute((TView)view, args);

    void ICommand.Execute(SceneBase view, params object[] args)
        => Execute((TView)view, args);
}