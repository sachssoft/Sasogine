using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Abstrakte Basis für Referenzen wie ComponentReference, AssetReference etc.
    /// AOT- und Trimming-sicher, keine Reflection.
    /// </summary>
    public abstract class ReferenceBase<T> where T : class
    {
        private T? _value;
        private string? _id;
        private bool _dirty = true;

        public event EventHandler? ValueChanged;

        /// <summary>
        /// Id des referenzierten Objekts
        /// </summary>
        public string? Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                _dirty = true;
            }
        }

        /// <summary>
        /// Zugriff auf die referenzierte Instanz
        /// </summary>
        public T? Value
        {
            get
            {
                if (_dirty)
                    Resolve();
                return _value;
            }
        }

        /// <summary>
        /// Prüft, ob die Referenz gültig ist
        /// </summary>
        public bool HasValue
        {
            get
            {
                if (_dirty)
                    Resolve();
                return _value != null;
            }
        }

        protected void SetValue(T? value)
        {
            if (ReferenceEquals(_value, value)) return;
            _value = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Muss von der konkreten Referenz implementiert werden
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected abstract T? ResolveValue(string id);

        private void Resolve()
        {
            _dirty = false;

            if (string.IsNullOrEmpty(_id))
            {
                SetValue(null);
                return;
            }

            var resolved = ResolveValue(_id);
            SetValue(resolved);
        }

        /// <summary>
        /// Erzwingt erneutes Auflösen (z.B. wenn Registry gewechselt wurde)
        /// </summary>
        protected void MarkDirty()
        {
            _dirty = true;
        }
    }
}