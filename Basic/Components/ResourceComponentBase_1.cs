using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ResourceComponentBase<TDefinition> : EngineObject<TDefinition>, IResourceComponent
        where TDefinition : class, IComponentDefinition
    {
        protected ResourceComponentBase() { }
    }
}