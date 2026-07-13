using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Tracks whether a value or definition has changed.
    /// </summary>
    public sealed class DirtyFlag
    {
        private bool _isDirty;

        /// <summary>
        /// Gets whether the flag is currently marked as dirty.
        /// </summary>
        public bool IsDirty => _isDirty;

        /// <summary>
        /// Marks the state as dirty.
        /// </summary>
        public void Mark()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Returns the current dirty state and clears the flag.
        /// </summary>
        public bool Consume()
        {
            bool result = _isDirty;
            _isDirty = false;
            return result;
        }

        /// <summary>
        /// Clears the dirty state without returning the previous value.
        /// </summary>
        public void Clear()
        {
            _isDirty = false;
        }
    }
}