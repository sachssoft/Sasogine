using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop : IWindowHost
    {
        IEnumerable<IWindowContent> IWindowHost.Windows =>
            (IEnumerable<IWindowContent>?)Widgets
                .OfType<IWindowContent>()
                .FirstOrDefault() ??
                    Enumerable.Empty<IWindowContent>();

        Rectangle IWindowHost.Bounds => LayoutBounds;

        void IWindowHost.AddWindow(IWindowContent window)
        {
            Widgets.Add((Widget)window);
        }

        void IWindowHost.RemoveWindow(IWindowContent window)
        {
            if (Widgets.Contains((Widget)window) == false)
                throw new InvalidOperationException("The specified window is not hosted by this window host.");

            Widgets.Remove((Widget)window);
        }
    }
}
