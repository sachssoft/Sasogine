using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using System.Collections.ObjectModel;

namespace Sachssoft.Sasogine.Surface.Controls;

public abstract class Container : Widget
{
    public virtual ObservableCollection<Widget> Widgets => Children;

    public Container()
    {
        HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
        VerticalAlignment = Visuals.VerticalAlignment.Stretch;
    }

    public override bool InputFallsThrough(Point localPos) => Background.Value == null;

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Container source)
            return;

        foreach (var child in source.Widgets)
        {
            var cloned = child.Clone();

            if (cloned is Widget clonedWidget)
                Widgets.Add(clonedWidget);
        }
    }
}