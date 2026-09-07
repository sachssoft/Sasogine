using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Provides a registry for entity definitions and their associated entity types.
    /// </summary>
    /// <remarks>
    /// The registry maps <see cref="IEntityDefinition"/> implementations to
    /// corresponding <see cref="IEntity"/> implementations for entity creation
    /// and definition resolution.
    /// </remarks>
    public sealed class EntityDefinitionRegistry :
        DefinitionRegistry<IEntity, IEntityDefinition>
    {
    }
}