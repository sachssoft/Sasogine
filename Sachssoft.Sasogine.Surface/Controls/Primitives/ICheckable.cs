using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface ICheckable
    {
        event EventHandler? IsCheckedChanged;

        bool IsChecked { get; set; }
    }
}
