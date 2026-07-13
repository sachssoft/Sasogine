using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Basic.Common
{
    /// <summary>
    /// Provides extension methods for working with definitions.
    /// </summary>
    public static class DefinitionExtensions
    {
        /// <summary>
        /// Marks a definition property as modified.
        /// </summary>
        /// <param name="definition">
        /// Definition to mark as dirty.
        /// </param>
        /// <param name="propertyName">
        /// Name of the modified property.
        /// </param>
        /// <param name="action">
        /// Dirty action associated with the modification.
        /// </param>
        public static void MarkDirty(
            this IDefinition definition,
            string propertyName,
            DirtyAction action = DirtyAction.None)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

            if (definition is not IHasDirtyMark hasDirtyMark)
                return;

            hasDirtyMark.Dirty.Add(propertyName, action);
        }
    }
}