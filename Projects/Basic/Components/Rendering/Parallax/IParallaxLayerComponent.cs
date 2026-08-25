namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    public interface IParallaxLayerComponent : IUpdatableComponent, IDrawableComponent
    {
        bool IsEnabled { get; set; }

        bool IsVisible { get; set; }

        void SetDrawOrder(int index);
    }
}
