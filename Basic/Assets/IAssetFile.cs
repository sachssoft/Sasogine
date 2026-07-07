using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    public interface IAssetFile : ICloneable
    {
        Type AssetType { get; }

        // Dateiname ohne Pfad
        string Name { get; }

        // Relativer Pfad zum Asset
        string RelativePath { get; }

        // Vollständiger relativer Pfad
        string FullRelativePath { get; }

        // Format konnte nicht erkannt werden
        bool IsUnknown { get; }

        // Laden oder Verarbeiten des Assets ist fehlgeschlagen
        Exception? Error { get; }

        // Erstellt das Asset anhand des Resolvers
        IAsset? Resolve(Stream stream, IAssetResolverProvider resolver);

        new IAssetFile Clone();
    }
}