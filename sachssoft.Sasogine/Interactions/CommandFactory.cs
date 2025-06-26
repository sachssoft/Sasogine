using sachssoft.Sasogine.Surface;
using System;

namespace sachssoft.Sasogine.Interactions;

public static class CommandFactory
{

    public static ICommand Create<TView>(Action<TView, object[]> execute)
        where TView : ViewBase
    {
        return new ActionCommand<TView>(execute, null);
    }

    public static ICommand Create<TView>(Action<TView, object[]> execute, Func<TView, object[], bool>? can_execute)
        where TView : ViewBase
    {
        return new ActionCommand<TView>(execute, can_execute);
    }

}
