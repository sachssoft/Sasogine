using sachssoft.Sasogine.Surface;
using System;

namespace sachssoft.Sasogine.Interactions;

public interface ICommand
{

    event EventHandler? CanExecuteChanged;

    void Execute(ViewBase? view, params object?[]? args);

    bool CanExecute(ViewBase? view, params object?[]? args);

}
