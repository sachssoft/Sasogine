using Sachssoft.Basic;
using Sachssoft.Sasogine.Surface.Interactions;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Interactions
{
    public static class Command
    {
        public static ICommand Create(Action<object?[]?> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            return new ActionCommand(execute, canExecute: null);
        }

        public static ICommand Create(
            Func<object?[]?, bool>? canExecute,
            Action<object?[]?> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            return new ActionCommand(execute, canExecute);
        }

        public static ICommand Create<TSender>(Action<TSender, object?[]?> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            return new BoundCommand<TSender>(execute, canExecute: null);
        }

        public static ICommand Create<TSender>(
            Func<TSender, object?[]?, bool>? canExecute,
            Action<TSender, object?[]?> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            return new BoundCommand<TSender>(execute, canExecute);
        }

        public static ICommand Create<TSender>(
            Func<TSender, ICommandRule> bindingLazy,
            Action<TSender, object?[]?> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            return new BoundCommand<TSender>(bindingLazy, execute);
        }

        public static ICommandRule Rule<TSender>(Func<TSender, object[], bool> canExecute)
        {
            return new CommandRule<TSender>(canExecute);
        }

        public static bool Require(object sender, object[] args, bool condition, params ICommandRule[] otherRules)
        {
            if (!condition) return false;
            foreach (var cond in otherRules)
                if (!cond.Evaluate(sender, args))
                    return false;
            return true;
        }

        public static bool RequireAny(object sender, object[] args, bool condition, params ICommandRule[] otherRules)
        {
            foreach (var cond in otherRules)
            {
                if (cond.Evaluate(sender, args))
                    return true;
            }
            return condition;
        }
    }
}
