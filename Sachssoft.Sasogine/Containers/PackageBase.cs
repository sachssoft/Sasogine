using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Containers;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

public abstract class PackageBase : IDisposable, IPackage
{
    protected private ZipArchive? _source;
    protected private Stream? _stream;
    private bool _isClosed = true;
    protected private bool _isReadOnly;
    protected private PackageManifest _manifest;
    private bool _isDisposed;
    private PackageLicense _license;
    private PackageIcon _icon;
    private PackagePreviews _previews;
    private IDocumentFormatter _manifestFormat = new JsonDocumentFormatter();
    private readonly Func<Stream> _openFunc;

    internal PackageBase(Func<Stream> streamFactory, bool isReadOnly, PackageManifest? manifest = null)
    {
        _openFunc = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        _isReadOnly = isReadOnly;

        _manifest = manifest ?? new PackageManifest();
        _manifest._package = this;

        _license = new PackageLicense(this);
        _icon = new PackageIcon(this);
        _previews = new PackagePreviews(this);
    }

    internal ZipArchive? Source => _source;

    /// <summary>
    /// Gets whether the package is currently open.
    /// </summary>
    public bool IsOpen => _source != null;

    /// <summary>
    /// Gets whether the package is read-only.
    /// </summary>
    public bool IsReadOnly => _isReadOnly;

    /// <summary>
    /// Gets the package manifest.
    /// </summary>
    public PackageManifest Manifest => _manifest;

    /// <summary>
    /// Gets or sets the formatter used for the manifest.
    /// </summary>
    [AllowNull]
    public IDocumentFormatter ManifestFormat
    {
        get => _manifestFormat;
        set => _manifestFormat = value ?? new JsonDocumentFormatter();
    }

    /// <summary>
    /// Gets the package license.
    /// </summary>
    public PackageLicense License => _license;

    /// <summary>
    /// Gets the package icon handler.
    /// </summary>
    public PackageIcon Icon => _icon;

    /// <summary>
    /// Gets the package previews handler.
    /// </summary>
    public PackagePreviews Previews => _previews;

    IReadOnlyDictionary<string, IPackageAsset> IPackage.Assets => new Dictionary<string, IPackageAsset>();

    IReadOnlyCollection<PackageLevelBase> IPackage.Levels => Array.Empty<PackageLevelBase>();

    /// <summary>
    /// Opens the package safely, throwing detailed exceptions if the operation fails.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if package is already open or stream is invalid.</exception>
    /// <exception cref="IOException">Thrown if stream cannot be accessed.</exception>
    public void Open()
    {
        ThrowIfDisposed();
        if (_source != null)
            throw new InvalidOperationException("Package is already open.");

        if (_isClosed)
        {
            _isClosed = false;

            try
            {
                _stream = _openFunc() ?? throw new IOException("Stream factory returned null.");
                _stream.Position = 0;

                if (!_isReadOnly && (!_stream.CanRead || !_stream.CanWrite || !_stream.CanSeek))
                    throw new PackageException("Cannot open package in edit mode.");

                _source = new ZipArchive(_stream, _isReadOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update);
                _manifest.Load();
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidOperationException("Package format is invalid or corrupted.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException("Failed to open package stream.", ex);
            }
        }
    }

    /// <summary>
    /// Closes the package and releases all underlying resources.
    /// </summary>
    public void Close()
    {
        ThrowIfDisposed();

        try
        {
            if (_source != null && !IsReadOnly)
                _manifest.Save();

            foreach (var level in _manifest._levels)
                level.Close();

            _source?.Dispose();
            _stream?.Dispose();
        }
        finally
        {
            _source = null;
            _stream = null;
            _isClosed = true;
        }
    }

    /// <summary>
    /// Saves the package by closing and reopening it.
    /// </summary>
    public void Save()
    {
        Close();                      // Schließt das Package/Zip-Archiv komplett
        Open();                       // Öffnet es wieder neu (z. B. um später weiter zu arbeiten)
    }

    /// <summary>
    /// Disposes the package and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
            return;

        foreach (var level in _manifest._levels)
            level.Dispose();

        _source?.Dispose();
        _stream?.Dispose();

        _source = null;
        _stream = null;
        _isClosed = true;
        _isDisposed = true;
    }

    internal bool IsFileExists(string filePath)
    {
        return Source?.Entries.Any(e =>
            e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)) ?? false;
    }

    internal Stream? OpenFile(string filePath)
    {
        return Source?.Entries
            .FirstOrDefault(e => e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
            ?.Open();
    }

    internal void MoveFileTo(string oldFilePath, string newFilePath)
    {
        ThrowIfDisposed();
        ThrowIfNotOpened();
        ThrowIfIsReadOnly();

        if (string.IsNullOrWhiteSpace(newFilePath))
            throw new ArgumentException("Target path cannot be empty.", nameof(newFilePath));

        if (!IsFileExists(oldFilePath))
            throw new PackageException($"Source file '{oldFilePath}' does not exist in the package.");

        if (IsFileExists(newFilePath))
            throw new PackageException($"Target file '{newFilePath}' already exists in the package.");

        var oldEntry = _source.GetEntry(oldFilePath);
        if (oldEntry == null)
            throw new PackageException($"Source entry '{oldFilePath}' not found in the ZIP archive.");

        try
        {
            var newEntry = _source.CreateEntry(newFilePath);

            using var oldStream = oldEntry.Open();
            using var newStream = newEntry.Open();
            oldStream.CopyTo(newStream);

            oldEntry.Delete();
        }
        catch (Exception ex)
        {
            throw new PackageException($"Failed to move file '{oldFilePath}' to '{newFilePath}'.", ex);
        }
    }


    internal void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(PackageBase));
    }

    [MemberNotNull(nameof(Source))]
    [MemberNotNull(nameof(_source))]
    internal void ThrowIfNotOpened()
    {
        if (Source == null && !_isDisposed)
            throw new PackageException("Package is not opened.");
    }

    internal void ThrowIfIsReadOnly()
    {
        if (IsReadOnly)
            throw new PackageException("Package is read-only.");
    }
}
