namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    internal interface IVectorSegmentInternal
    {
        VectorPath? Owner { get; set; }

        void OnOwnerChanged(VectorPath? oldOwner, VectorPath? newOwner);
    }
}