namespace Sachssoft.Sasogine.Engine
{
    public interface IRuntimeComponent : IComponent
    {
        void Update(RuntimeContext context);

    }
}
