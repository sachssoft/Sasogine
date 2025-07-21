using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace sachssoft.Sasogine.Resources;

public class PackageBase : IDisposable
{
    private ZipArchive? _zip;
    private Stream? _stream;
    private ZipArchiveMode _mode;

    private readonly byte[]? _aesKey;
    private readonly byte[]? _aesIv;

    private List<PackageEntry>? _entries;

    public string? ManifestContent { get; private set; }

    public bool CanUpdate => _mode == ZipArchiveMode.Update;

    public PackageBase(byte[]? aesKey = null, byte[]? aesIv = null)
    {
        _aesKey = aesKey;
        _aesIv = aesIv;
    }

    public void Open(Stream stream, ZipArchiveMode mode = ZipArchiveMode.Read)
    {
        if (_zip != null)
            throw new InvalidOperationException("Package already opened.");

        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _mode = mode;
        _zip = new ZipArchive(_stream, _mode, leaveOpen: true);

        var manifestEntry = _zip.GetEntry("manifest.json");
        if (manifestEntry == null)
            throw new FileNotFoundException("Manifest not found in package.");

        using var sr = new StreamReader(manifestEntry.Open(), Encoding.UTF8);
        ManifestContent = sr.ReadToEnd();

        LoadEntries();
    }

    private void LoadEntries()
    {
        if (_zip == null)
            throw new InvalidOperationException("Package not opened.");

        _entries = new List<PackageEntry>();

        foreach (var entry in _zip.Entries)
        {
            if (entry.FullName == "manifest.json")
                continue;

            _entries.Add(new PackageEntry(_zip, entry, isEncrypted: false, version: null, _aesKey, _aesIv));
        }
    }

    public IReadOnlyList<PackageEntry> GetEntries()
    {
        if (_entries == null)
            throw new InvalidOperationException("Package not opened.");

        return _entries.AsReadOnly();
    }

    public PackageEntry? GetEntry(string filePath)
    {
        return _entries?.FirstOrDefault(e => e.FilePath == filePath);
    }

    public void Dispose()
    {
        _zip?.Dispose();
        _stream?.Dispose();
        _zip = null;
        _stream = null;
        _entries = null;
    }
}
