//using System;

//namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

//public static class CommandFactory
//{
//    public static ICommand Create(string label, Action<object> execute)
//    {
//        return new SurfaceCommand(execute, null);
//    }

//    public static ICommand Create(string label, Action<object> execute, Func<object, bool> can_execute)
//    {
//        return new SurfaceCommand(execute, can_execute);
//    }

//    public static ICommand Create(string label, Action<object> execute, Func<object, bool> can_execute, bool is_enabled)
//    {
//        return new SurfaceCommand(execute, can_execute)
//        {
//            IsEnabled = is_enabled
//        };
//    }

//    public static ICommand CreateCheckable(string label, Action<object> execute, bool is_checked)
//    {
//        return new CheckableSurfaceCommand(execute, null)
//        {
//            IsChecked = is_checked
//        };
//    }

//    public static ICommand CreateCheckable(string label, Action<object> execute, Func<object, bool> can_execute, bool is_checked)
//    {
//        return new CheckableSurfaceCommand(execute, can_execute)
//        {
//            IsChecked = is_checked
//        };
//    }

//    public static ICommand CreateCheckable(string label, Action<object> execute, Func<object, bool> can_execute, bool is_checked, bool is_enabled)
//    {
//        return new CheckableSurfaceCommand(execute, can_execute)
//        {
//            IsEnabled = is_enabled,
//            IsChecked = is_checked
//        };
//    }
//}
