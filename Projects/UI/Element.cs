using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Represents element.</summary>
public abstract class Element : INotifyPropertyChanged
{
    /// <summary>Occurs after an effective property value changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Updates a field and raises a caller-supplied cached notification.</summary>
    protected bool SetNotify<T>(ref T field, T value, PropertyChangedEventArgs notification)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        NotifyPropertyChanged(notification);
        return true;
    }

    /// <summary>Raises a cached notification and invalidates the owning control.</summary>
    protected void NotifyPropertyChanged(PropertyChangedEventArgs notification)
    {
        OnPropertyChanged(notification);
        PropertyChanged?.Invoke(this, notification);
    }

    /// <summary>Allows derived elements to invalidate dependent state.</summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
    }

    internal void CollectionChanged(System.ComponentModel.PropertyChangedEventArgs notification) => NotifyPropertyChanged(notification);
}
