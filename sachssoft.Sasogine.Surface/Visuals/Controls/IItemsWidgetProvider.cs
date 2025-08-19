using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public interface IItemsWidgetProvider
{

    IList<Widget> Widgets { get; }

    IEnumerable<Widget>? WidgetsSource { get; set; }
}
