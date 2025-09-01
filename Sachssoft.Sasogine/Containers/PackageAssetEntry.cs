using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Resources;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace Sachssoft.Sasogine.Containers;

public sealed class PackageAssetEntry : IPackageAsset
{
    private readonly PackageBase _package;
    private AssetCategory _category = AssetCategory.Other;
    private string _categoryName = AssetCategory.Other.ToString();

    internal PackageAssetEntry(PackageBase package)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
    }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;

    // Type Factory !! -> TypedFactoryManager CreateInstance (TypeKey)
    public string? TypeFactoryKey { get; set; }

    public IAssetProvider? Asset { get; set; }

    /// <summary>
    /// Vollständiger Pfad im Package, z. B. "assets/textures/foo.png"
    /// </summary>
    public string FilePath => ProjectedPackageAssetCollection.FILE_PATH + FileName;

    public AssetCategory Category
    {
        get => _category;
        set
        {
            _category = value;
            _categoryName = _category.ToString(); // immer synchronisieren
        }
    }

    public string CategoryName
    {
        get => _categoryName;
        set
        {
            _categoryName = value;

            if (Enum.TryParse<AssetCategory>(_categoryName, true, out var cat))
                _category = cat;
            else
                _category = AssetCategory.Other;
        }
    }

    public long Size { get; internal set; }

    public string? Hash { get; set; }

    public Stream Open()
    {
        _package.ThrowIfNotOpened();
        var entry = EnsureGetEntry();

        return entry.Open();
    }

    public void Delete()
    {
        _package.ThrowIfNotOpened();
        var entry = EnsureGetEntry();

        entry.Delete();

        _package.Manifest._assets.Remove(FileName);
        _package.Manifest.Save();
    }

    public void Replace(Stream stream)
    {
        _package.ThrowIfNotOpened();
        var oldEntry = EnsureGetEntry();

        oldEntry.Delete();

        var newEntry = _package.Source.CreateEntry(FilePath);
        using var entryStream = newEntry.Open();

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        stream.CopyTo(entryStream);

        Size = entryStream.Length;

        _package.Manifest._assets[FileName] = this;
        _package.Manifest.Save();
    }

    internal ZipArchiveEntry EnsureGetEntry()
    {
        var entry = _package.Source!.GetEntry(FilePath);
        if (entry == null)
            throw new FileNotFoundException($"Asset '{FileName}' not found in package.");

        return entry;
    }
}
