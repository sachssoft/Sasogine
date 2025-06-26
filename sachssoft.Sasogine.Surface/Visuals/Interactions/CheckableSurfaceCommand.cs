//using System;

//namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

//public class CheckableSurfaceCommand : SurfaceCommand, ICheckableCommandCore
//{
//    private bool _is_checked;

//    public event EventHandler? IsCheckedChanged;

//    public CheckableSurfaceCommand(Action<object[]> execute, Func<object[], bool>? canExecute = null)
//        : base(execute, canExecute)
//    {
//    }

//    public bool IsChecked
//    {
//        get => _is_checked;
//        set
//        {
//            if (_is_checked != value)
//            {
//                _is_checked = value;
//                IsCheckedChanged?.Invoke(this, EventArgs.Empty);
//            }
//        }
//    }
//}
