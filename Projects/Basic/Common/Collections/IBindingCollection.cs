using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Common.Collections;

/// <summary>
/// Represents a mutable binding collection that provides change notifications.
/// </summary>
/// <typeparam name="T">
/// The type of object contained in the collection.
/// </typeparam>
public interface IBindingCollection<T> :
    IList<T>,
    INotifyCollectionChanged,
    INotifyPropertyChanged
{
}