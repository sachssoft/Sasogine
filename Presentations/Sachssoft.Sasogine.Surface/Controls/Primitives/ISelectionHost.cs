using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface ISelectionHost
    {
        bool AllowMultiple { get; }

        IEnumerable<ISelectable> Selectables { get; }
    }
}
