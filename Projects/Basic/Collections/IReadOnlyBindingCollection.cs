using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Engine.Collections;

/// <summary>
/// Represents a read-only binding collection that provides change notifications.
/// </summary>
/// <typeparam name="T">
/// The type of object contained in the collection.
/// </typeparam>
public interface IReadOnlyBindingCollection<out T> :
    IReadOnlyList<T>,
    INotifyCollectionChanged,
    INotifyPropertyChanged
{
}