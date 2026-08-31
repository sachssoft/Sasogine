using Sachssoft.Sasogine.Components.Models;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides the base definition for engine objects.
    ///
    /// Defines common configuration properties such as the unique identifier
    /// and optional object classification.
    /// </summary>
    public class EngineObjectDefinition : IEngineObjectDefinition
    {
        /// <summary>
        /// Gets or sets the unique identifier of the engine object.
        ///
        /// A new identifier is generated automatically when the definition is created.
        /// </summary>
        [Category(Categories.Common)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the optional classification or category of the engine object.
        /// </summary>
        [Category(Categories.Common)]
        public string? Class { get; set; }
    }
}