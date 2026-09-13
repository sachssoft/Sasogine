using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    public sealed class VectorShapeDefinition : IDefinition
    {
        public bool IsLocked { get; set; }

        public VectorPathDefinition[]? Paths { get; set; }
    }
}
