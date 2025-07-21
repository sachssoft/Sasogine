using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace sachssoft.Sasogine.Resources;

public class PackageEntry
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly byte[]? _aesKey;
    private readonly byte[]? _aesIv;

    public string FileName => _entry.Name;
    public string FilePath => _entry.FullName;
    public bool IsEncrypted { get; }
    public Version? Version { get; }
    public long CompressedSize => _entry.CompressedLength;
    public long UncompressedSize => _entry.Length;
    public DateTime LastModified => _entry.LastWriteTime.DateTime;

    internal PackageEntry(ZipArchive archive, ZipArchiveEntry entry, bool isEncrypted, Version? version, byte[]? aesKey, byte[]? aesIv)
    {
        _archive = archive ?? throw new ArgumentNullException(nameof(archive));
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));
        IsEncrypted = isEncrypted;
        Version = version;
        _aesKey = aesKey;
        _aesIv = aesIv;
    }

    /// <summary>
    /// Öffnet den Inhalt dieser Datei als lesbaren Stream.
    /// Bei verschlüsselten Dateien wird ein entschlüsselnder Stream zurückgegeben.
    /// </summary>
    /// <returns>Lesbarer Stream</returns>
    /// <exception cref="InvalidOperationException">Wenn AES-Parameter fehlen</exception>
    public Stream Open()
    {
        Stream baseStream = _entry.Open();

        if (IsEncrypted)
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("AES key or IV not set for encrypted file.");

            var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = _aesIv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            return new CryptoStream(baseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        }

        return baseStream;
    }

    public bool TryOpen(out Stream? stream)
    {
        try
        {
            stream = Open();
            return true;
        }
        catch
        {
            stream = null;
            return false;
        }
    }

    /// <summary>
    /// Löscht diese Datei aus dem ZIP-Archiv.
    /// </summary>
    /// <exception cref="InvalidOperationException">Wenn das Archiv nicht im Update-Modus geöffnet wurde.</exception>
    public void Delete()
    {
        if (_archive.Mode != ZipArchiveMode.Update)
            throw new InvalidOperationException("Cannot delete entries unless the package is opened in Update mode.");

        _entry.Delete();
    }

    public bool TryDelete()
    {
        if (_archive.Mode != ZipArchiveMode.Update)
            return false;

        try
        {
            _entry.Delete();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ersetzt den Inhalt dieser Datei im ZIP-Archiv.
    /// Achtung: Die aktuelle PackageEntry-Instanz bleibt auf dem alten Eintrag stehen.
    /// </summary>
    /// <param name="newContent">Neuer Inhalt als Stream (wird vollständig gelesen)</param>
    /// <exception cref="InvalidOperationException">Wenn das Archiv nicht im Update-Modus ist.</exception>
    public void Replace(Stream newContent)
    {
        if (_archive.Mode != ZipArchiveMode.Update)
            throw new InvalidOperationException("Cannot replace entries unless the package is opened in Update mode.");

        if (newContent == null)
            throw new ArgumentNullException(nameof(newContent));

        // Alte Datei löschen
        _entry.Delete();

        // Neue Datei hinzufügen mit gleichem Pfad
        var newEntry = _archive.CreateEntry(_entry.FullName);

        using var entryStream = newEntry.Open();
        if (newContent.CanSeek)
            newContent.Position = 0;

        newContent.CopyTo(entryStream);

        // Hinweis: Diese PackageEntry verweist weiter auf den alten, gelöschten Eintrag.
        // Um darauf zuzugreifen, sollte die Eintragsliste im PackageBase neu geladen werden.
    }

    public bool TryReplace(Stream newContent)
    {
        if (_archive.Mode != ZipArchiveMode.Update || newContent == null)
            return false;

        try
        {
            _entry.Delete();

            var newEntry = _archive.CreateEntry(_entry.FullName);
            using var entryStream = newEntry.Open();
            if (newContent.CanSeek)
                newContent.Position = 0;
            newContent.CopyTo(entryStream);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
