namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public interface IParallaxLayerComponent : IUpdatableComponent, IDrawableComponent
    {
        void SetDrawOrder(int index);
    }
}
