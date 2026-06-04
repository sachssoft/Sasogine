using Sachssoft.Sasogine.Common;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Components
{
    public abstract class ComponentBase<TDefinition> : EngineObject<TDefinition>, IResourceComponent
        where TDefinition : class, IComponentDefinition
    {
        protected ComponentBase() { }
    }
}