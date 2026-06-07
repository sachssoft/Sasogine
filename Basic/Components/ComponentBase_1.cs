using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ComponentBase<TDefinition> : EngineObject<TDefinition>, IResourceComponent
        where TDefinition : class, IComponentDefinition
    {
        protected ComponentBase() { }
    }
}