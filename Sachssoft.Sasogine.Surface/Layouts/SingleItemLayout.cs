using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Surface.Layouts;

public class SingleItemLayout<T> : ILayout where T : Widget
{
    private readonly Widget _container;

    private ObservableCollection<Widget> Children => _container.Children;

    public T? Child
    {
        get { return Children.Count > 0 ? (T)Children[0] : null; }
        set
        {
            Children.Clear();

            if (value != null)
            {
                Children.Add(value);
            }
        }
    }

    public SingleItemLayout(Widget container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public SingleItemLayout(Widget container, T? child)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        Child = child;
    }

    public Point Measure(IEnumerable<Widget> widgets, Point availableSize)
    {
        var result = Mathematics.PointZero;

        if (Child != null)
        {
            result = Child.Measure(availableSize);
        }

        return result;
    }

    public void Arrange(IEnumerable<Widget> widgets, Rectangle bounds)
    {
        if (Child != null && Child.IsVisible)
        {
            Child.Arrange(bounds);
        }
    }
}
