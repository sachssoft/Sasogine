using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    public interface IVectorVariableSegment : IVectorSegment
    {
        VectorNodeCollection ControlNodes { get; }
    }
}