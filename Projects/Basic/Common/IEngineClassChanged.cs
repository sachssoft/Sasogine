using System;

namespace Sachssoft.Engine.Common
{
    /// <summary>
    /// Provides notification when the engine classification changes.
    /// </summary>
    public interface IEngineClassChanged
    {
        /// <summary>
        /// Occurs when the classification of the engine object changes.
        /// </summary>
        event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;
    }
}