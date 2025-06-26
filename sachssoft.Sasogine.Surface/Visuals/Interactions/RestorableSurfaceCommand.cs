//using System;

//namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

//public class RestorableSurfaceCommand : SurfaceCommand, IRestorableCommandCore
//{
//    private readonly Action<object[]>? _unexecute;

//    protected RestorableSurfaceCommand()
//        : base()
//    {
//        _unexecute = null;
//    }

//    public RestorableSurfaceCommand(
//        Action<object[]> execute,
//        Action<object[]> unexecute,
//        Func<object[], bool>? can_execute = null)
//        : base(execute, can_execute)
//    {
//        _unexecute = unexecute ?? throw new ArgumentNullException(nameof(unexecute));
//    }

//    public virtual void Unexecute(params object[] args)
//    {
//        _unexecute?.Invoke(args);
//    }
//}