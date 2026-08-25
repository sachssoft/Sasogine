using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class VectorPathSegmentsEventArgs : EventArgs
    {
        public VectorPathSegmentsEventArgs(
            IReadOnlyList<IToolVectorSegment> segments)
        {
            Segments = segments;
        }

        public IReadOnlyList<IToolVectorSegment> Segments { get; }
    }
}