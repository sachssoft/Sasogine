using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ResourceComponentBase<TDefinition> : EngineObject<TDefinition>, IResourceComponent
        where TDefinition : class, IComponentDefinition, new()
    {
        protected ResourceComponentBase() { }
    }
}