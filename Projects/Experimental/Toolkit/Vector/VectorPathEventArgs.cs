using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Provides data for an event involving a vector path.
    /// </summary>
    public sealed class VectorPathEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPathEventArgs"/> class
        /// using the specified vector path.
        /// </summary>
        /// <param name="path">
        /// The vector path associated with the event.
        /// </param>
        public VectorPathEventArgs(
            VectorPath path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the vector path associated with the event.
        /// </summary>
        public VectorPath Path { get; }
    }
}