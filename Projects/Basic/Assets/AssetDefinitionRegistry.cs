using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Provides a registry for asset definitions and their associated asset types.
/// </summary>
public sealed class AssetDefinitionRegistry :
    DefinitionRegistry<IAsset, IAssetDefinition>
{
}