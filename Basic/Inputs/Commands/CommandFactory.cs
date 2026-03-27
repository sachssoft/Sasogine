using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Inputs
{
    /// <summary>
    /// Zentrale Factory für Commands: Action, Delayed, Queue
    /// </summary>
    public static class CommandFactory
    {
        #region ActionCommand Factory

        // Vollständig mit CanExecute und EventHandler
        public static ICommand CreateAction(
            Action<object?> execute,
            Func<object?, bool>? canExecute,
            EventHandler? canExecuteChangedHandlers)
        {
            return new ActionCommand(execute, canExecute, canExecuteChangedHandlers);
        }

        // Mit CanExecute, ohne EventHandler
        public static ICommand CreateAction(
            Action<object?> execute,
            Func<object?, bool>? canExecute)
        {
            return new ActionCommand(execute, canExecute);
        }

        // Nur Execute
        public static ICommand CreateAction(Action<object?> execute)
        {
            return new ActionCommand(execute);
        }

        #endregion

        #region DelayedCommand Factory

        // Vollständig: Execute + Delay + CanExecute + EventHandler
        public static ICommand CreateDelayed(
            Action<object?> execute,
            int delayMilliseconds,
            Func<object?, bool>? canExecute,
            EventHandler? canExecuteChangedHandlers)
        {
            return new DelayedCommand(execute, delayMilliseconds, canExecute, canExecuteChangedHandlers);
        }

        // Ohne EventHandler
        public static ICommand CreateDelayed(
            Action<object?> execute,
            int delayMilliseconds,
            Func<object?, bool>? canExecute)
        {
            return new DelayedCommand(execute, delayMilliseconds, canExecute);
        }

        // Nur Execute + Delay
        public static ICommand CreateDelayed(Action<object?> execute, int delayMilliseconds)
        {
            return new DelayedCommand(execute, delayMilliseconds);
        }

        #endregion

        #region CommandQueue Factory

        /// <summary>
        /// Erstellt eine CommandQueue für Struct-Commands (ActionCommand oder DelayedCommand)
        /// </summary>
        /// <typeparam name="T">Struct-Command, der ICommand implementiert</typeparam>
        /// <param name="capacity">Vor-allozierte Kapazität</param>
        public static CommandQueue CreateQueue(int capacity = 1024)
        {
            return new CommandQueue(capacity);
        }

        #endregion
    }
}