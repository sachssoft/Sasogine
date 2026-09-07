using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines the base contract for entity definitions used by the Sasogine
    /// world system.
    /// </summary>
    /// <remarks>
    /// Entity definitions describe the persistent configuration of entities and
    /// provide the engine object metadata required by
    /// <see cref="IEngineObjectDefinition"/>.
    /// </remarks>
    public interface IEntityDefinition : IEngineObjectDefinition
    {
    }
}