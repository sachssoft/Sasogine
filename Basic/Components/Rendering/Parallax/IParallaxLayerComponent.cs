namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public interface IParallaxLayerComponent : IDrawableRuntimeComponent
    {
        void SetDrawOrder(int index);
    }
}
