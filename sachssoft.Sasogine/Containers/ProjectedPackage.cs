using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Documents.Xml;
using Sachssoft.Sasogine.Containers;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

public class ProjectedPackage : PackageBase
{
    private readonly ProjectedPackageLevelCollection _levels;
    private readonly ProjectedPackageAssetCollection _assets;

    public ProjectedPackageAssetCollection Assets => _assets;

    public override IReadOnlyCollection<PackageLevelBase> Levels => throw new System.NotImplementedException();

    public ProjectedPackage(Stream stream, PackageManifest? manifest = null)
        : base(stream, false, manifest)
    {
        _assets = new ProjectedPackageAssetCollection(this);
    }

    public ProjectedPackage(string filePath, PackageManifest? manifest = null)
        : base(File.Open(filePath, FileMode.Open, FileAccess.ReadWrite), false, manifest)
    {
        _assets = new ProjectedPackageAssetCollection(this);
    }

    public static ProjectedPackage Create(string filePath, PackageManifest? manifest = null, IDocumentFormatter formatter = null)
    {
        if (File.Exists(filePath))
            throw new InvalidOperationException("File already exists");

        // 1. Leere ZIP anlegen
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            // ZIP-Struktur schreiben
        }

        // 2. Neu öffnen im Update-Modus (kein using!)
        var fs2 = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        var package = Open(fs2, manifest);
        package.Manifest.Save();

        return package;
    }

    public static TPackage Create<TPackage, TManifest>(
        string filePath,
        Func<Stream, TManifest?, TPackage> createInstance,
        TManifest? manifest = null,
        IDocumentFormatter? formatter = null)
        where TPackage : ProjectedPackage
        where TManifest : PackageManifest
    {
        if (File.Exists(filePath))
            throw new InvalidOperationException("File already exists");

        // 1. Leere ZIP anlegen
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            // Struktur leer anlegen
        }

        // 2. Stream öffnen für Update
        var fs2 = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

        // 3. Instanz durch Factory erzeugen
        var package = createInstance(fs2, manifest);
        if (package == null)
            throw new InvalidOperationException("createInstance returned null");

        // 4. Package initialisieren
        package.Open();
        package.ManifestFormat = formatter;
        package.Manifest.Save();

        return package;
    }


    public static ProjectedPackage Create(string filePath, PackageManifest? manifest = null, PackageManifestDocumentType type = PackageManifestDocumentType.Json)
    {
        return Create(filePath, manifest, TypeToFormatter(type));
    }

    public static TPackage Create<TPackage, TManifest>(
       string filePath,
       Func<Stream, TManifest?, TPackage> createInstance,
       TManifest? manifest = null,
       PackageManifestDocumentType type = PackageManifestDocumentType.Json)
       where TPackage : ProjectedPackage
       where TManifest : PackageManifest
    {
        return Create<TPackage,TManifest>(filePath, createInstance, manifest, TypeToFormatter(type));
    }

    private static IDocumentFormatter TypeToFormatter(PackageManifestDocumentType type)
    {
        return type switch
        {
            PackageManifestDocumentType.Json => new JsonDocumentFormatter(),
            PackageManifestDocumentType.XML => new XmlDocumentFormatter(),
            _ => throw new NotSupportedException("Unknown document type is not supported")
        };
    }

    public override IPackageAsset? GetAsset(string filePath)
    {
        return _assets[filePath]!;
    }


    public static ProjectedPackage Open(Stream stream, PackageManifest? manifest = null)
    {
        var package = new ProjectedPackage(stream, manifest);
        package.Open();
        return package;
    }

    public static ProjectedPackage Open(string filePath, PackageManifest? manifest = null)
    {
        var package = new ProjectedPackage(filePath, manifest);
        package.Open();
        return package;
    }
}
