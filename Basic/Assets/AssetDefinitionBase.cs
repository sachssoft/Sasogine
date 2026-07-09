namespace Sachssoft.Sasogine.Assets;

public abstract class AssetDefinitionBase<T> : IAssetDefinition
    where T : class, IAsset
{
    public string? Id { get; set; }

    public string? Class { get; set; }

    public AssetFile<T>? File { get; set; }

    IAssetFile? IAssetDefinition.File
    {
        get => File;
        set => File = (AssetFile<T>?)value;
    }
}
