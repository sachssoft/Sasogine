using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Defines the configuration used to create a shader asset.
/// </summary>
/// <remarks>
/// Specifies a context-aware template used to create the runtime
/// <see cref="IShader"/> instance.
/// </remarks>
public class ShaderAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the template used to create the shader instance using
    /// the current <see cref="AssetContext"/>.
    /// </summary>
    public Template<IShader, ShaderAssetContext>? Template { get; set; }
}