using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface IWindowHost
    {
        Rectangle Bounds { get; }

        IEnumerable<IWindowContent> Windows { get; }

        void AddWindow(IWindowContent window);

        void RemoveWindow(IWindowContent window);
    }
}
