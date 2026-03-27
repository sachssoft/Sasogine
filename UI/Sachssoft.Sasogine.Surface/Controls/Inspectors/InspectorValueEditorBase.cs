using Sachssoft.Sasofly.Inspection;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    /// <summary>
    /// Basis-Klasse für alle Inspector-Value-Editoren.
    /// Verwaltet Property, Source und Inspector.
    /// </summary>
    public abstract class InspectorValueEditorBase : IDisposable
    {
        private bool _valueChangedByHere = false;

        /// <summary>
        /// Zugehöriger Inspector
        /// </summary>
        public Inspector Inspector { get; internal set; }

        /// <summary>
        /// Die Property, die dieser Editor bearbeitet
        /// </summary>
        public IProperty Property { get; internal set; }

        /// <summary>
        /// Das Quellobjekt, auf dem die Property liegt
        /// </summary>
        public NotifyingObject Source { get; internal set; }

        internal Func<object?>? TypeFactory { get; set; }

        protected object? CreateInstance(object?[]? args)
            => TypeFactory?.Invoke();

        /// <summary>
        /// Liefert den aktuellen Wert der Property.
        /// </summary>
        protected object? GetValue()
        {
            if (Source == null || Property == null) return null;
            return Source.GetValue(Property);
        }

        /// <summary>
        /// Setzt den Wert der Property.
        /// </summary>
        protected void SetValue(object? value)
        {
            if (Source == null || Property == null) return;
            _valueChangedByHere = true;
            Source.SetValue(Property, value);
            _valueChangedByHere = false;
        }

        protected internal virtual bool AllowNullable => false;

        protected internal virtual object? CreateInstance() => null;

        /// <summary>
        /// Baut das GUI-Widget für den Editor.
        /// </summary>
        protected internal abstract Widget BuildControl();

        protected internal virtual Container? BuildContainerAtBottom()
            => null;

        internal void Initialize()
        {
            Source.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_valueChangedByHere && e.PropertyName == Property.Name)
            {
                OnValueChangedBySource();
            }
        }

        protected virtual void OnValueChangedBySource()
        {
        }

        public void Dispose()
        {
            OnDisposed();
            Source.PropertyChanged -= Source_PropertyChanged;
        }

        protected virtual void OnDisposed() { }
    }
}
