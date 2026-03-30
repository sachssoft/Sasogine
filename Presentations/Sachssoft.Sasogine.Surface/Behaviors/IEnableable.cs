using Sachssoft.Sasogine.Surface.Styles;

namespace Sachssoft.Sasogine.Surface.Behaviors;

public interface IEnableable
{
    StyleProperty<bool> IsEnabled { get; set; }
}
