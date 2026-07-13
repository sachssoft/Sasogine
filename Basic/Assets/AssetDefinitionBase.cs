namespace Sachssoft.Sasogine.Assets;

public abstract class AssetDefinitionBase<T> : IAssetDefinition
    where T : class, IAsset
{
    public string? Id { get; set; }

    public string? Class { get; set; }

    public TypedAssetFile<T>? File { get; set; }

    IAssetFile? IAssetDefinition.File
    {
        get => File;
        set => File = (TypedAssetFile<T>?)value;
    }
}
