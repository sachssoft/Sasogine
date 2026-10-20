using Sachssoft.Engine.Assets.Graphics;

namespace Sachssoft.Engine.Controls;
/// <summary>Describes a registered object without loading its resource during enumeration.</summary>
public sealed class AssetItem
{
    private readonly Func<AssetPreview?>? _preview;
    private bool _requested;
    private AssetPreview? _cached;
    /// <summary>Creates metadata and an optional lazy preview factory, called on the UI thread.</summary>
    public AssetItem(IEngineReferenceable target, string name, string kind = "Asset", Func<AssetPreview?>? preview = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(target.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(kind);
        Target = target;
        Id = target.Id;
        Name = name;
        Kind = kind;
        _preview = preview;
    }
    /// <summary>Gets the registered object; browsing does not resolve or load it.</summary>
    public IEngineReferenceable Target { get; }
    /// <summary>Gets the stable identifier captured when this entry was created.</summary>
    public string Id { get; }
    /// <summary>Gets the displayed name.</summary>
    public string Name { get; }
    /// <summary>Gets the application-provided asset kind.</summary>
    public string Kind { get; }
    /// <summary>Gets the cached preview without loading during drawing.</summary>
    public AssetPreview? Preview => _cached;
    /// <summary>Gets a preview failure, or null if the request succeeded or has not run.</summary>
    public Exception? PreviewError { get; private set; }
    /// <summary>Loads the preview at most once until explicitly invalidated. Textures remain caller-owned.</summary>
    public void RequestPreview()
    {
        if (_requested)
            return;
        _requested = true;
        try { _cached = _preview?.Invoke(); }
        catch (Exception error) { PreviewError = error; }
    }
    /// <summary>Invalidates a stale or failed borrowed preview; the next visible update retries.</summary>
    public void InvalidatePreview()
    {
        _requested = false;
        _cached = null;
        PreviewError = null;
    }
    /// <summary>Creates a texture-asset entry whose resource is loaded only when its preview is requested.</summary>
    public static AssetItem FromTexture(Texture2DAsset asset, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return new AssetItem(asset, name ?? asset.Id ?? "Texture", "Texture", () => new AssetPreview(asset.GetOrLoad()));
    }
}
