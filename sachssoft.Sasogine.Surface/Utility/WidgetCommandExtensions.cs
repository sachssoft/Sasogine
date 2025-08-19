using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Utility;

public static class WidgetCommandExtensions
{
    public static void TryExecuteCommand<TCommandSource>(this TCommandSource source) where TCommandSource : ICommandSource
    {
        if (source.Command != null)
        {
            if (source is IEnableable enableable)
            {
                if (!enableable.IsEnabled)
                    return;
            }

            if (!source.Command.CanExecute(source.ViewOwner, source.CommandParameter))
            {
                return;
            }

            source.Command.Execute(source.ViewOwner, source.CommandParameter);
        }
    }
}
