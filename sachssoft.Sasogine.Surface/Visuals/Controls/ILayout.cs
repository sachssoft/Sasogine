using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public interface ILayout
{
    Point Measure(IEnumerable<Widget> widgets, Point availableSize);
    void Arrange(IEnumerable<Widget> widgets, Rectangle bounds);
}
