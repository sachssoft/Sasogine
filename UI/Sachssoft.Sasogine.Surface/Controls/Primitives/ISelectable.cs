using Sachssoft.Sasogine.Surface.Behaviors;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface ISelectable
    {
        event EventHandler<ValueChangedEventArgs<bool>>? IsSelectedChanged;
        
        bool IsSelectable { get; set; }

        bool IsSelected { get; set; }
    }
}
