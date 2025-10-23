using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Interactions;

public interface ICommand
{

    event EventHandler? CanExecuteChanged;

    void Execute(SceneBase? view, params object?[]? args);

    bool CanExecute(SceneBase? view, params object?[]? args);

}
