using Sachssoft.Sasogine.Presentation;

namespace Sachssoft.Sasogine.Interactions;

public interface ICommandSource
{
    ICommand? Command { get; set; }

    object? CommandParameter { get; set; }

    SceneBase? ViewOwner { get; set; }
}
