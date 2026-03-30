using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public interface ICommandRule
    {
        bool Evaluate(object? sender, object?[]? args);

        void Bind(ICommand command);

        void NotifyCanExecuteChanged();
    }
}
