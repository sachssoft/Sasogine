using Sachssoft.Engine.Common;
using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets
{
    /// <summary>
    /// Defines the common contract for asset definitions.
    /// </summary>
    /// <remarks>
    /// Asset definitions describe the configuration used to create
    /// and initialize assets.
    /// </remarks>
    public interface IAssetDefinition : IEngineObjectDefinition
    {
        /// <summary>
        /// Gets or sets the file from which the asset is loaded.
        /// </summary>
        [Category(Categories.Common)]
        [DisplayName("File")]
        public IAssetFile? File { get; set; }
    }
}