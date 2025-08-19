using Sachssoft.Sasogine.Surface;

namespace Sachssoft.Sasogine.Interactions;

public interface ICommandSource
{
    ICommand? Command { get; set; }

    object? CommandParameter { get; set; }

    ViewBase? ViewOwner { get; set; }
}
