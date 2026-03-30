using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Interactions
{
    public interface IRaiseCanExecuteChangedCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
