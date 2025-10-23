using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Interactions;

public static class CommandFactory
{

    public static ICommand Create<TView>(Action<TView, object[]> execute)
        where TView : SceneBase
    {
        return new ActionCommand<TView>(execute, null);
    }

    public static ICommand Create<TView>(Action<TView, object[]> execute, Func<TView, object[], bool>? can_execute)
        where TView : SceneBase
    {
        return new ActionCommand<TView>(execute, can_execute);
    }

}
