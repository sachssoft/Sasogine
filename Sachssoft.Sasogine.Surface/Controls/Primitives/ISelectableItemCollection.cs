using Sachssoft.Sasogine.Surface.Behaviors;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface ISelectableItemPresenterCollection : IReorderableList, INotifyCollectionChanged
    {
        bool TryGetPresenter(object? content, out SelectableItem? presenter);

        SelectableItem GetPresenterByIndex(int index);

        IEnumerable<SelectableItem> GetPresenters();
    }
}
