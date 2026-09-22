namespace Sachssoft.Engine.Components.Tools;

internal interface IVectorSegmentInternal
{
    VectorPath? Owner { get; set; }

    void OnOwnerChanged(VectorPath? oldOwner, VectorPath? newOwner);
}