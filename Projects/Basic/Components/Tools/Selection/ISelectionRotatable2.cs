using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be rotated by the Selection Tool.
    /// </summary>
    public interface ISelectionRotatable2 : ISelectionTarget2, ITransformRotation2, ITransformRotationPivot2
    {
        /// <summary>
        /// Gets a value indicating whether rotation is allowed.
        /// </summary>
        bool AllowRotate { get; }
    }
}