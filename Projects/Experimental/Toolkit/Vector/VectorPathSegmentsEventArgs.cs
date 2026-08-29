using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Provides data for an event involving multiple segments of a vector path.</summary>
    public sealed class VectorPathSegmentsEventArgs : EventArgs
    {
        public VectorPathSegmentsEventArgs(
            IReadOnlyList<IVectorSegment> segments)
        {
            Segments = segments;
        }

        /// <summary>Gets the vector segments associated with the event.</summary>
        public IReadOnlyList<IVectorSegment> Segments { get; }
    }
}