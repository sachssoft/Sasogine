using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Controls.Inspectors;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop : IModalHost
    {
        IEnumerable<IModalContent> IModalHost.Modals =>
            (IEnumerable<IModalContent>?)Widgets
                .OfType<IModalContent>()
                .FirstOrDefault() ??
                    Enumerable.Empty<IModalContent>();

        Rectangle IModalHost.Bounds => LayoutBounds;

        void IModalHost.AddModal(IModalContent modal)
        {
            if (modal.ModalBackground != null)
            {
                Widgets.Add(new ModalScreen()
                {
                    IsHitTestVisible = true,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new Styles.StyleProperty<IBrush?>(modal.ModalBackground),
                    Owner = modal,
                });
            }

            Widgets.Add((Widget)modal);
        }

        void IModalHost.RemoveModal(IModalContent modal)
        {
            if (Widgets.Contains((Widget)modal) == false)
                throw new InvalidOperationException("The specified modal is not hosted by this modal host.");

            Widgets.Remove((Widget)modal);

            var background = Widgets.OfType<ModalScreen>()
                                    .Where(x => x.Owner == modal)
                                    .FirstOrDefault();

            if (background != null)
            {
                Widgets.Remove((Widget)background);
            }
        }
    }
}
