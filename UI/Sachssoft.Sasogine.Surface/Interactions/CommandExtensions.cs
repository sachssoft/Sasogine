using Sachssoft.Sasogine.Interactions;
using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public static class CommandExtensions
    {
        public static void RaiseCanExecuteChanged(this IRaiseCanExecuteChangedCommand command)
        {
            command.RaiseCanExecuteChanged();
        }

        public static void TryRaiseCanExecuteChanged(this ICommand command)
        {
            if (command is IRaiseCanExecuteChangedCommand r)
            {
                r.RaiseCanExecuteChanged();
            }
        }

        public static bool CanExecute(this ICommandSource commandSource)
        {
            return commandSource.Command?.CanExecute(
                new object?[] {
                    commandSource.CommandOwner,
                    commandSource.CommandParameter
                }
            ) ?? true;
        }

        public static void Execute(this ICommandSource commandSource)
        {
            if (commandSource.Command == null)
                return;

            if (commandSource.CanExecute())
            {
                commandSource.Command.Execute(
                    new object?[] {
                        commandSource.CommandOwner,
                        commandSource.CommandParameter
                    }
                );
            }
        }
    }
}
