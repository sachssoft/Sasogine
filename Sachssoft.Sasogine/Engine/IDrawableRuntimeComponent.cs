namespace Sachssoft.Sasogine.Engine
{
    public interface IDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(RuntimeViewportContext context);
    }
}
