using System;

namespace Sachssoft.Engine
{
    /// <summary>
    /// Provides notification when the identifier of an engine referenceable changes.
    /// </summary>
    public interface IEngineReferenceableChanged
    {
        /// <summary>
        /// Occurs when the identifier changes.
        /// </summary>
        event EventHandler<EngineObjectChangedEventArgs>? IdChanged;
    }
}
