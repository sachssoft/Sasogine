using System.Collections.ObjectModel;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface IMenuNode
    {
        ObservableCollection<MenuItemBase> Items { get; }
    }
}
