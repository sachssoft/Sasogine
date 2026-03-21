using Sachssoft.Sasogine.Surface.Controls.Primitives;
using System.Drawing;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public abstract class MenuItemBase : CommandItemBase<Menu, MenuItemBase>
    {
        private IMenuNode? _parent;

        #region Direct Properties

        public IMenuNode? Parent
        {
            get => _parent;
            internal set
            {
                if (SetAndNotify(ref _parent, value))
                {
                }
            }
        }

        #endregion
    }
}
