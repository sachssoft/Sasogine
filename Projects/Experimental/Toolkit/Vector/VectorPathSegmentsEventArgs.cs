using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Provides data for an event involving multiple segments of a vector path.
    /// </summary>
    public sealed class VectorPathSegmentsEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPathSegmentsEventArgs"/> class
        /// using the specified vector segments.
        /// </summary>
        /// <param name="segments">
        /// The vector segments associated with the event.
        /// </param>
        public VectorPathSegmentsEventArgs(
            IReadOnlyList<IVectorSegment> segments)
        {
            Segments = segments;
        }

        /// <summary>
        /// Gets the vector segments associated with the event.
        /// </summary>
        public IReadOnlyList<IVectorSegment> Segments { get; }
    }
}