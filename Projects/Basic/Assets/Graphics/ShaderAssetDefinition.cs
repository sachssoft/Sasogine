using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Defines the configuration used to create a shader asset.
/// </summary>
public class ShaderAssetDefinition : AssetDefinitionBase<ShaderAsset>
{
    /// <summary>
    /// Gets or sets the template used to create the shader instance.
    /// </summary>
    public Template<IShader>? Template { get; set; }
}