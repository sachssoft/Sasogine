namespace Sachssoft.Sasogine.Assets;

public abstract class AssetDefinitionBase : IAssetDefinition
{
    public string? Id { get; set; }

    public string? Class { get; set; }

    public string? RelativeFullPath { get; set; }
}
