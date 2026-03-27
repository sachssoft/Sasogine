using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Controls;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Layouts;

public interface ILayout
{
    Point Measure(IEnumerable<Widget> widgets, Point availableSize);

    void Arrange(IEnumerable<Widget> widgets, Rectangle bounds);
}
