using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls.Focus
{
    public interface IFocusHost
    {
        void Next();

        void Previous();

        IEnumerable<IFocusChild> GetChildren();

        IFocusChild? CurrentChild { get; }

    }
}
