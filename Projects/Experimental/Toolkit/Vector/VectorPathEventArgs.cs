using System;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Provides data for an event involving a vector path.</summary>
    public sealed class VectorPathEventArgs : EventArgs
    {
        public VectorPathEventArgs(VectorPath path)
        {
            Path = path;
        }

        /// <summary>Gets the vector path associated with the event.</summary>
        public VectorPath Path { get; }
    }
}