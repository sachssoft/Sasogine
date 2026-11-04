using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Sachssoft.Engine.Packages;

/// <summary>
/// Reads and writes a versioned container with application metadata and named entries.
/// </summary>
/// <remarks>
/// Format 1 stores four application-defined magic bytes, a format byte, a length-prefixed
/// UTF-8 package version, a metadata length and block, and an entry count. Each entry
/// stores its UTF-8 path length and bytes, codec byte, decoded and stored lengths, then
/// its payload. Lengths and counts are little-endian signed 32-bit integers.
/// </remarks>
public sealed class PackageContainer : IDisposable
{
    /// <summary>The on-disk layout understood by this implementation.</summary>
    private const byte FormatVersion = 1;

    /// <summary>The maximum decoded or stored size of one entry.</summary>
    private const int MaxEntryLength = 64 * 1024 * 1024;

    /// <summary>The maximum number of entries in a container.</summary>
    private const int MaxEntries = 65_535;

    /// <summary>The maximum byte length of the metadata block.</summary>
    private const int MaxMetadataLength = 1024 * 1024;

    /// <summary>The maximum UTF-8 byte length of an entry path.</summary>
    private const int MaxPathLength = 4096;

    /// <summary>The strict encoding used for container text fields.</summary>
    private static readonly UTF8Encoding TextEncoding = new(false, true);

    /// <summary>The index of entry offsets, lengths, and codecs.</summary>
    private readonly Dictionary<string, (long Offset, int StoredLength, int Length, byte Codec)> _entries;

    /// <summary>Whether disposal leaves the input stream open.</summary>
    private readonly bool _leaveOpen;

    /// <summary>The uncompressed metadata bytes.</summary>
    private readonly byte[] _metadata;

    /// <summary>Serializes access to the shared input stream.</summary>
    private readonly object _streamLock = new();

    /// <summary>The seekable stream holding the container.</summary>
    private readonly Stream _source;

    /// <summary>Whether the container has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a container from a validated index.</summary>
    /// <param name="source">The stream that holds the container.</param>
    /// <param name="leaveOpen">Whether to leave the stream open on disposal.</param>
    /// <param name="version">The application package version.</param>
    /// <param name="metadata">The raw metadata bytes.</param>
    /// <param name="entries">The validated entry index.</param>
    private PackageContainer(
        Stream source,
        bool leaveOpen,
        Version version,
        byte[] metadata,
        Dictionary<string, (long Offset, int StoredLength, int Length, byte Codec)> entries)
    {
        _source = source;
        _leaveOpen = leaveOpen;
        Version = version;
        _metadata = metadata;
        _entries = entries;
    }

    /// <summary>Gets the case-sensitive relative paths in the container.</summary>
    public IEnumerable<string> EntryPaths => _entries.Keys;

    /// <summary>Gets the package version stored separately from the metadata.</summary>
    public Version Version { get; }

    /// <summary>Opens a container without checking its application-specific identifier.</summary>
    /// <param name="source">A stream whose remaining bytes contain one complete container.</param>
    /// <param name="leaveOpen">Whether disposal leaves the source open.</param>
    /// <returns>The indexed container. Ownership transfers only after a successful open.</returns>
    public static PackageContainer Open(Stream source, bool leaveOpen = false)
    {
        return OpenCore(source, ReadOnlySpan<byte>.Empty, leaveOpen);
    }

    /// <summary>Opens a container at the current position of a readable, seekable stream.</summary>
    /// <param name="source">A stream whose remaining bytes contain one complete container.</param>
    /// <param name="expectedMagic">The four application-defined identification bytes.</param>
    /// <param name="leaveOpen">Whether disposal leaves the source open.</param>
    /// <returns>The indexed container. Ownership transfers only after a successful open.</returns>
    public static PackageContainer Open(Stream source, ReadOnlySpan<byte> expectedMagic, bool leaveOpen = false)
    {
        if (expectedMagic.Length != 4)
            throw new ArgumentException("The package identifier must contain four bytes.", nameof(expectedMagic));

        return OpenCore(source, expectedMagic, leaveOpen);
    }

    /// <summary>Writes an unencrypted container at the current destination position.</summary>
    /// <param name="destination">The writable destination, which remains open.</param>
    /// <param name="magic">The four application-defined identification bytes.</param>
    /// <param name="version">The package version written as UTF-8 text.</param>
    /// <param name="metadata">The required metadata bytes, stored without interpretation.</param>
    /// <param name="entries">Entry contents keyed by case-sensitive relative paths using forward slashes.</param>
    public static void Write(
        Stream destination,
        ReadOnlySpan<byte> magic,
        Version version,
        ReadOnlySpan<byte> metadata,
        IReadOnlyDictionary<string, ReadOnlyMemory<byte>> entries)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(entries);

        if (magic.Length != 4)
            throw new ArgumentException("The package identifier must contain four bytes.", nameof(magic));

        if (metadata.IsEmpty || metadata.Length > MaxMetadataLength)
            throw new ArgumentOutOfRangeException(nameof(metadata));

        if (entries.Count > MaxEntries)
            throw new ArgumentOutOfRangeException(nameof(entries));

        foreach (var entry in entries)
        {
            if (!IsValidPath(entry.Key) || TextEncoding.GetByteCount(entry.Key) > MaxPathLength ||
                entry.Value.Length > MaxEntryLength)
            {
                throw new ArgumentException("An entry has an invalid path or size.", nameof(entries));
            }
        }

        using var writer = new BinaryWriter(destination, TextEncoding, leaveOpen: true);
        writer.Write(magic);
        writer.Write(FormatVersion);

        byte[] versionBytes = TextEncoding.GetBytes(version.ToString());
        writer.Write((byte)versionBytes.Length);
        writer.Write(versionBytes);
        writer.Write(metadata.Length);
        writer.Write(metadata);
        writer.Write(entries.Count);

        foreach (var entry in entries)
        {
            byte[] path = TextEncoding.GetBytes(entry.Key);
            writer.Write(path.Length);
            writer.Write(path);

            using var compressed = new MemoryStream();
            using (var encoder = new BrotliStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                encoder.Write(entry.Value.Span);

            bool useCompression = compressed.Length < entry.Value.Length;
            writer.Write(useCompression ? (byte)1 : (byte)0);
            writer.Write(entry.Value.Length);
            if (useCompression)
            {
                writer.Write((int)compressed.Length);
                writer.Write(compressed.GetBuffer().AsSpan(0, (int)compressed.Length));
            }
            else
            {
                writer.Write(entry.Value.Length);
                writer.Write(entry.Value.Span);
            }
        }
    }

    /// <summary>Releases the container and closes the source unless it was left open.</summary>
    public void Dispose()
    {
        lock (_streamLock)
        {
            if (_disposed)
                return;

            _disposed = true;
            if (!_leaveOpen)
                _source.Dispose();
        }
    }

    /// <summary>Opens an entry as an independent, read-only stream.</summary>
    /// <param name="path">The exact relative entry path, using forward slashes.</param>
    /// <returns>A stream over the entry bytes, valid after container disposal.</returns>
    public Stream OpenEntry(string path)
    {
        byte[] stored;
        (long Offset, int StoredLength, int Length, byte Codec) entry;

        lock (_streamLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_entries.TryGetValue(path, out entry))
                throw new FileNotFoundException("The package entry was not found.", path);

            stored = new byte[entry.StoredLength];
            _source.Position = entry.Offset;
            _source.ReadExactly(stored);
        }

        if (entry.Codec == 0)
            return new MemoryStream(stored, writable: false);

        byte[] decoded = new byte[entry.Length];
        using var decoder = new BrotliDecoder();
        var status = decoder.Decompress(stored, decoded, out int consumed, out int written);
        if (status != OperationStatus.Done || consumed != stored.Length || written != decoded.Length)
            throw new InvalidDataException("Invalid Brotli package entry.");

        return new MemoryStream(decoded, writable: false);
    }

    /// <summary>Opens the raw metadata without reading any entry payloads.</summary>
    /// <returns>An independent, read-only stream over the metadata bytes.</returns>
    public Stream OpenMetadata()
    {
        lock (_streamLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return new MemoryStream(_metadata, writable: false);
        }
    }

    /// <summary>Checks whether a path consists of nonempty relative segments.</summary>
    /// <param name="path">The entry path to validate.</param>
    /// <returns>Whether the path is a valid container entry path.</returns>
    private static bool IsValidPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path.Contains('\\') || path.Contains(':') || path.Contains('\0'))
            return false;

        foreach (string segment in path.Split('/'))
        {
            if (segment.Length == 0 || segment is "." or "..")
                return false;
        }

        return true;
    }

    /// <summary>Reads the container header and indexes its entries.</summary>
    /// <param name="source">The readable, seekable container stream.</param>
    /// <param name="expectedMagic">The expected identifier, or an empty span to accept any identifier.</param>
    /// <param name="leaveOpen">Whether disposal leaves the source open.</param>
    /// <returns>The indexed container.</returns>
    private static PackageContainer OpenCore(Stream source, ReadOnlySpan<byte> expectedMagic, bool leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead || !source.CanSeek)
            throw new ArgumentException("The source must be readable and seekable.", nameof(source));

        using var reader = new BinaryReader(source, TextEncoding, leaveOpen: true);
        byte[] magic = ReadBlock(reader, 4);
        if (!expectedMagic.IsEmpty && !magic.AsSpan().SequenceEqual(expectedMagic))
            throw new InvalidDataException("The package identifier does not match.");

        if (reader.ReadByte() != FormatVersion)
            throw new InvalidDataException("Unsupported package container format.");

        string versionText = TextEncoding.GetString(ReadBlock(reader, reader.ReadByte()));
        if (!System.Version.TryParse(versionText, out Version? version))
            throw new InvalidDataException("Invalid package version.");

        int metadataLength = ReadLength(reader, MaxMetadataLength);
        if (metadataLength == 0)
            throw new InvalidDataException("The package metadata block is empty.");

        byte[] metadata = ReadBlock(reader, metadataLength);
        int count = ReadLength(reader, MaxEntries);
        var entries = new Dictionary<string, (long Offset, int StoredLength, int Length, byte Codec)>(StringComparer.Ordinal);

        for (int index = 0; index < count; index++)
        {
            string path = TextEncoding.GetString(ReadBlock(reader, ReadLength(reader, MaxPathLength)));
            byte codec = reader.ReadByte();
            int length = ReadLength(reader, MaxEntryLength);
            int storedLength = ReadLength(reader, MaxEntryLength);
            long offset = source.Position;

            if (!IsValidPath(path) || codec > 1 ||
                (codec == 0 && length != storedLength) ||
                (codec == 1 && (storedLength == 0 || storedLength >= length)) ||
                storedLength > source.Length - offset ||
                !entries.TryAdd(path, (offset, storedLength, length, codec)))
            {
                throw new InvalidDataException("Invalid or duplicate package entry.");
            }

            source.Position = offset + storedLength;
        }

        if (source.Position != source.Length)
            throw new InvalidDataException("Unexpected data after the package entries.");

        return new PackageContainer(source, leaveOpen, version, metadata, entries);
    }

    /// <summary>Reads a complete block and rejects truncated input.</summary>
    /// <param name="reader">The reader positioned at the block.</param>
    /// <param name="length">The expected byte count.</param>
    /// <returns>The block bytes.</returns>
    private static byte[] ReadBlock(BinaryReader reader, int length)
    {
        byte[] bytes = new byte[length];
        reader.BaseStream.ReadExactly(bytes);
        return bytes;
    }

    /// <summary>Reads and bounds a length or count from the container.</summary>
    /// <param name="reader">The reader positioned at the value.</param>
    /// <param name="maximum">The maximum permitted value.</param>
    /// <returns>The validated value.</returns>
    private static int ReadLength(BinaryReader reader, int maximum)
    {
        int length = reader.ReadInt32();
        if (length < 0 || length > maximum)
            throw new InvalidDataException("Package length or count is out of range.");

        return length;
    }
}
