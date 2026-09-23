using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.World;

/// <summary>
/// Provides a registry for creating entities from entity definitions.
/// </summary>
/// <remarks>
/// The registry maps <see cref="IEntityDefinition"/> implementations to
/// corresponding <see cref="IEntity"/> implementations for entity creation.
/// </remarks>
public sealed class EntityDefinitionRegistry :
    DefinitionObjectRegistry<IEntityDefinition, IEntity>
{
}