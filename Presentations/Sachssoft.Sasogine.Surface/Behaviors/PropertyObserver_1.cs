using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Behaviors
{
    public class PropertyObserver<T> : IDisposable where T : class, INotifyPropertyChanged
    {
        private readonly T _source;
        private readonly Dictionary<string, List<Action<T>>> _bindings = new();
        private readonly Dictionary<string, List<ICommand>> _commandBindings = new();
        private bool _disposed;

        internal PropertyObserver(T source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _source.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Bindet eine Property an eine Callback-Aktion.
        /// </summary>
        public PropertyObserver<T> Bind(string propertyName, Action<T> callback)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PropertyObserver<T>));

            if (!_bindings.TryGetValue(propertyName, out var list))
            {
                list = new List<Action<T>>();
                _bindings[propertyName] = list;
            }

            list.Add(callback);
            return this;
        }


        public PropertyObserver<T> Bind(string propertyName, ICommand command)
        {
            Bind(propertyName, [command]);
            return this;
        }


        public PropertyObserver<T> Bind(string propertyName, ICommandRule commandRule)
        {
            Bind(propertyName, [commandRule]);
            return this;
        }

        /// <summary>
        /// Bindet eine Property an mehrere ICommand-Instanzen.
        /// Bei jeder Änderung wird CanExecute überprüft.
        /// Duplikate werden automatisch entfernt.
        /// </summary>
        public PropertyObserver<T> Bind(string propertyName, ICommand[] commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            // Nur eindeutige Commands
            var uniqueCommands = new HashSet<ICommand>(commands);

            Bind(propertyName, (source) =>
            {
                foreach (var command in uniqueCommands)
                    command.TryRaiseCanExecuteChanged(); // Extension!
            });

            return this;
        }
        public PropertyObserver<T> Bind(string propertyName, ICommandRule[] commandRules)
        {
            if (commandRules == null) throw new ArgumentNullException(nameof(commandRules));

            // Nur eindeutige Commands
            var uniqueRules = new HashSet<ICommandRule>(commandRules);

            Bind(propertyName, (source) =>
            {
                foreach (var rule in uniqueRules)
                    rule.NotifyCanExecuteChanged();
            });

            return this;
        }

        /// <summary>
        /// Bindet eine Property an ein ICommand. 
        /// Bei jeder Änderung wird CanExecute überprüft.
        /// </summary>
        public PropertyObserver<T> Bind(string propertyName, Action<T> callback, ICommand[] commands)
        {
            Bind(propertyName, callback);
            Bind(propertyName, commands);
            return this;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposed || e.PropertyName == null) return;

            // Callbacks
            if (_bindings.TryGetValue(e.PropertyName, out var list))
            {
                foreach (var item in list)
                    item.Invoke(_source);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _source.PropertyChanged -= OnPropertyChanged;
                _bindings.Clear();
                _commandBindings.Clear();
                _disposed = true;
            }
        }
    }
}
