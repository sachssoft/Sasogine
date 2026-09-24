using Sachssoft.Engine;

namespace Sachssoft.Engine.World
{
    /// <summary>
    /// Provides runtime context information used to initialize entities.
    /// </summary>
    /// <remarks>
    /// Implementations can expose runtime services and resources required by
    /// entities during initialization.
    /// </remarks>
    public interface IEntityContext : IEngineObjectContext
    {
    }
}