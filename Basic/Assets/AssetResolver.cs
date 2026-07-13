using System;
using System.Collections.Generic;
using System.IO;
using Sachssoft.Sasogine.Assets.Audio;
using Sachssoft.Sasogine.Assets.Graphics;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Resolves asset files into strongly typed <see cref="IAssetFile"/> instances
    /// based on the contents of a stream.
    /// </summary>
    public class AssetResolver : IAssetResolverProvider
    {
        private readonly List<ResolverEntry> _entries = new();

        /// <summary>
        /// Represents a registered asset resolver entry.
        /// </summary>
        private sealed class ResolverEntry
        {
            /// <summary>
            /// Gets the function used to determine whether the stream matches this resolver.
            /// </summary>
            public Func<Stream, bool> Match { get; init; } = default!;

            /// <summary>
            /// Gets the factory function used to create the corresponding asset file.
            /// </summary>
            public Func<string, IAssetFile> Factory { get; init; } = default!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetResolver"/> class
        /// and registers the default asset resolvers.
        /// </summary>
        public AssetResolver()
        {
            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Png,
                p => new TypedAssetFile<Texture2DAsset>(p));

            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Jpeg,
                p => new TypedAssetFile<Texture2DAsset>(p));

            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Wav,
                p => new TypedAssetFile<SoundAsset>(p));

            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Ogg,
                p => new TypedAssetFile<MusicAsset>(p));
        }

        /// <summary>
        /// Registers a new asset resolver.
        /// </summary>
        /// <param name="match">
        /// Function that determines whether the stream matches the asset type.
        /// </param>
        /// <param name="factory">
        /// Function that creates the corresponding asset file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="match"/> or <paramref name="factory"/> is null.
        /// </exception>
        public void Register(Func<Stream, bool> match, Func<string, IAssetFile> factory)
        {
            ArgumentNullException.ThrowIfNull(match);
            ArgumentNullException.ThrowIfNull(factory);

            _entries.Add(new ResolverEntry
            {
                Match = match,
                Factory = factory
            });
        }

        /// <summary>
        /// Resolves a typed asset file from a relative path and stream.
        /// </summary>
        /// <param name="relativePath">
        /// Relative path of the asset inside the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <returns>
        /// A typed asset file if a matching resolver is found; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> or <paramref name="stream"/> is null.
        /// </exception>
        public IAssetFile? Resolve(string relativePath, Stream stream)
        {
            ArgumentNullException.ThrowIfNull(relativePath);
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                long position = stream.CanSeek ? stream.Position : 0;

                bool matches = entry.Match(stream);

                if (stream.CanSeek)
                    stream.Position = position;

                if (matches)
                    return entry.Factory(relativePath);
            }

            return null;
        }
    }
}