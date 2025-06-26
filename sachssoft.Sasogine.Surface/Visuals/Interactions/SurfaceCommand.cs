//using System;

//namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

//public class SurfaceCommand : ICommand, IActivatableCommand
//{
//    private readonly Action<object[]>? _execute;
//    private readonly Func<object[], bool>? _can_execute;
//    private bool _is_enabled = true;

//    public event EventHandler? CanExecuteChanged;

//    protected SurfaceCommand()
//    {
//        _execute = null;
//        _can_execute = null;
//    }

//    public SurfaceCommand(Action<object[]> execute, Func<object[], bool>? can_execute = null)
//    {
//        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//        _can_execute = can_execute;
//    }

//    public virtual bool CanExecute(params object[] args)
//    {
//        if (!IsEnabled)
//            return false;

//        return _can_execute?.Invoke(args) ?? true;
//    }

//    public virtual void Execute(params object[] args)
//    {
//        if (!CanExecute(args))
//            return;

//        _execute?.Invoke(args);
//    }

//    public void RaiseCanExecuteChanged()
//    {
//        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
//    }

//    public string? Label { get; set; }

//    public bool IsEnabled
//    {
//        get => _is_enabled;
//        set
//        {
//            if (_is_enabled != value)
//            {
//                _is_enabled = value;
//                RaiseCanExecuteChanged();
//            }
//        }
//    }
//}
