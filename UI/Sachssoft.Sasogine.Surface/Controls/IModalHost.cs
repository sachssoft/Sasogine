using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public interface IModalHost
    {
        Rectangle Bounds { get; }

        IEnumerable<IModalContent> Modals { get; }

        void AddModal(IModalContent modal);

        void RemoveModal(IModalContent modal);
    }
}
