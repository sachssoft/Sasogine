using System;

namespace Sachssoft.Sasogine.World.Tiles
{
    /// <summary>
    /// Represents a runtime tile object stored in a tile map.
    /// A tile object contains a definition and can create independent copies of itself.
    /// </summary>
    public interface ITileObject : ICloneable
    {
        /// <summary>
        /// Gets the definition that describes this tile object.
        /// </summary>
        ITileDefinition Definition { get; }

        /// <summary>
        /// Creates a copy of this tile object.
        /// </summary>
        /// <returns>A cloned tile object.</returns>
        new ITileObject Clone();
    }
}