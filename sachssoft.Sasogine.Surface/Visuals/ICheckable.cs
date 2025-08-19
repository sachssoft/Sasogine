using System;

namespace Sachssoft.Sasogine.Surface.Visuals;

public interface ICheckable
{
    event EventHandler? IsCheckedChanged;

    bool IsChecked { get; set; }

    bool IsCheckable { get; set; }
}
