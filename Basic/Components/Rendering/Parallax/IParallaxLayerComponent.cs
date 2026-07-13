namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public interface IParallaxLayerComponent : IUpdatableComponent, IDrawableComponent
    {
        bool IsEnabled { get; set; }

        bool IsVisible { get; set; }

        void SetDrawOrder(int index);
    }
}
