using sachssoft.Sasogine.Surface;

namespace sachssoft.Sasogine.Interactions;

public interface ICommandSource
{
    ICommand? Command { get; set; }

    object? CommandParameter { get; set; }

    ViewBase? ViewOwner { get; set; }
}
