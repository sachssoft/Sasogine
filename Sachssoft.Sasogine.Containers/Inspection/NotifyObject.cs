using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Inspection
{
    public class NotifyObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Occurs when the <see cref="Id"/> property value changes.
        /// </summary>
        public event EventHandler? IdChanged;

        private string? _id;
        private object? _dataContext;
        private bool _silent;

        /// <summary>
        /// Gets or sets the unique identifier of this object.
        /// Raises the <see cref="IdChanged"/> event when modified.
        /// </summary>
        public string? Id
        {
            get => _id;
            set
            {
                if (SetAndNotify(ref _id, value))
                    OnIdChanged();
            }
        }

        /// <summary>
        /// Gets or sets the data context for the current object, which provides a source of data for data binding
        /// operations.
        /// </summary>
        /// <remarks>Setting this property updates the data context used by bindings within the object and
        /// its child elements. Changing the data context may affect how bound properties and controls display or
        /// interact with data. This property is commonly used in UI frameworks to facilitate binding between UI
        /// elements and underlying data models.</remarks>
        public object? DataContext
        {
            get => _dataContext;
            set => SetAndNotify(ref _dataContext, value);
        }

        /// <summary>
        /// Called when the <see cref="Id"/> property changes.
        /// Invokes the <see cref="IdChanged"/> event.
        /// </summary>
        protected internal virtual void OnIdChanged() => IdChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Sets the specified field and raises property change notifications if the value differs.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">The new value to assign.</param>
        /// <param name="silent">If true, no events are raised.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns><c>true</c> if the value changed; otherwise, <c>false</c>.</returns>
        protected bool SetAndNotify<T>(ref T? field, T? value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            if (_silent)
            {
                field = value;
                return false;
            }

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected bool SetAndNotify<T>(ref T? field, T? value, out T? oldValue, [CallerMemberName] string? propertyName = null)
        {
            oldValue = field;

            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            if (_silent)
            {
                field = value;
                return false;
            }

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event for the specified property.
        /// Does nothing if notifications are suppressed.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            if (_silent) return;
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// Does nothing if notifications are suppressed.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (_silent) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Temporarily suppresses all property change notifications within a <see cref="using"/> scope.
        /// Automatically restores the previous state when disposed.
        /// </summary>
        /// <returns>A disposable scope that disables notifications.</returns>
        public IDisposable SuppressNotifications() => new SilentScope(this);

        /// <summary>
        /// Represents a disposable scope that temporarily disables property change notifications.
        /// </summary>
        private class SilentScope : IDisposable
        {
            private readonly NotifyObject _obj;
            private readonly bool _previous;

            /// <summary>
            /// Initializes a new <see cref="SilentScope"/> and disables notifications.
            /// </summary>
            /// <param name="obj">The parent object.</param>
            public SilentScope(NotifyObject obj)
            {
                _obj = obj;
                _previous = obj._silent;
                obj._silent = true;
            }

            /// <summary>
            /// Restores the previous notification state when disposed.
            /// </summary>
            public void Dispose() => _obj._silent = _previous;
        }
    }
}
