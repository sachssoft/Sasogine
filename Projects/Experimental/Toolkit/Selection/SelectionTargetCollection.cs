//using Sachssoft.Sasogine.Components.Tools.Selection;
//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Sachssoft.Sasogine.Components.Tools;

//public class SelectionTargetCollection : IList<ISelectionTarget>
//{
//    private readonly List<ISelectionTarget> _items = new();

//    public ISelectionTarget this[int index]
//    {
//        get => _items[index];
//        set
//        {
//            ArgumentNullException.ThrowIfNull(value);
//            _items[index] = value;
//        }
//    }

//    public int Count => _items.Count;

//    public bool IsReadOnly => false;

//    public virtual void Add(ISelectionTarget item)
//    {
//        ArgumentNullException.ThrowIfNull(item);
//        _items.Add(item);
//    }

//    public virtual void Add(object item)
//    {
//        ArgumentNullException.ThrowIfNull(item);

//        if (item is not ISelectionTarget target)
//        {
//            throw new ArgumentException(
//                $"The specified object must implement {nameof(ISelectionTarget)}.",
//                nameof(item));
//        }

//        Add(target);
//    }

//    public virtual void Clear()
//        => _items.Clear();

//    public bool Contains(ISelectionTarget item)
//        => _items.Contains(item);

//    public void CopyTo(
//        ISelectionTarget[] array,
//        int arrayIndex)
//        => _items.CopyTo(array, arrayIndex);

//    public IEnumerator<ISelectionTarget> GetEnumerator()
//        => _items.GetEnumerator();

//    public int IndexOf(ISelectionTarget item)
//        => _items.IndexOf(item);

//    public virtual void Insert(
//        int index,
//        ISelectionTarget item)
//    {
//        ArgumentNullException.ThrowIfNull(item);
//        _items.Insert(index, item);
//    }

//    public virtual bool Remove(ISelectionTarget item)
//        => _items.Remove(item);

//    public virtual void RemoveAt(int index)
//        => _items.RemoveAt(index);

//    IEnumerator IEnumerable.GetEnumerator()
//        => GetEnumerator();
//}