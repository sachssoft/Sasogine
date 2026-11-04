using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Localization;
using System.IO;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Loads a translation table from an XML resource source.
/// </summary>
public sealed class LocalizedDictionaryAsset : AssetBase<LocalizedDictionary, LocalizedDictionaryAssetDefinition>
{
    /// <summary>
    /// Creates a translation asset backed by an XML source.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The source of the XML document.</param>
    public LocalizedDictionaryAsset(string? id, ResourceSourceBase loaderSource)
        : base(new LocalizedDictionaryAssetDefinition { Id = id })
    {
        LoaderSource = loaderSource ?? throw new System.ArgumentNullException(nameof(loaderSource));
    }

    /// <inheritdoc/>
    protected override LocalizedDictionary Build(Stream stream)
    {
        return LocalizedDictionary.ReadXml(stream);
    }
}
