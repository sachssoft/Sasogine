using Sachssoft.Sasogine.Surface.Controls;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls;

public interface IItemsWidgetProvider
{
    IList<Widget> Widgets { get; }

    IEnumerable<Widget>? WidgetsSource { get; set; }
}
