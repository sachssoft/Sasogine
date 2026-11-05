namespace Sachssoft.Engine.Controls;

/// <summary>Supplies an invertible visual transformation for a control and its descendants.</summary>
public interface IControlTransform
{
    /// <summary>Gets the transformation of absolute layout coordinates before ancestor transforms.</summary>
    /// <remarks>Implementations must return a finite invertible affine matrix. Layout bounds remain
    /// untransformed; rendering and routed pointer coordinates use the same transformation.</remarks>
    Matrix VisualTransform
    {
        get;
    }
}
