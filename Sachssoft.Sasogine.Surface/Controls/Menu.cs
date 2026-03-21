using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using System.Collections.ObjectModel;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class Menu : CommandBarBase<Menu, MenuItemBase>, IMenuNode
    {
        public ObservableCollection<MenuItemBase> Items => throw new System.NotImplementedException();

        public Menu()
        {
            IconWidth = IconWidth.Override(16);
            IconHeight = IconHeight.Override(16);
        }

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new Menu();
        }

        #endregion
    }
}
