using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public interface ICommandSource
    {
        ICommand? Command { get; set; }

        object? CommandParameter { get; set; }

        object? CommandOwner { get; set; }
    }
}
