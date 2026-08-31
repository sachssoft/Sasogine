using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides notifications when the identity or classification
    /// of an engine object changes.
    /// </summary>
    public interface IEngineObjectIdentityChanged
    {
        /// <summary>
        /// Occurs when the identifier of the engine object changes.
        /// </summary>
        event EventHandler<EngineObjectChangedEventArgs>? IdChanged;

        /// <summary>
        /// Occurs when the classification of the engine object changes.
        /// </summary>
        event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;
    }
}