using Sachssoft.Sasogine.Assets.Audio;
using Sachssoft.Sasogine.Assets.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Detects asset formats and creates the corresponding asset files
    /// and asset definitions.
    /// </summary>
    public class AssetResolver : IAssetResolverProvider
    {
        private readonly List<ResolverEntry> _entries = new();

        private sealed class ResolverEntry
        {
            public Type DefinitionType { get; init; } = default!;

            public Func<Stream, bool> Match { get; init; } = default!;

            public Func<string, IAssetFile> FileFactory { get; init; } = default!;

            public Func<IAssetDefinition> DefinitionFactory { get; init; } = default!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetResolver"/> class
        /// and registers the default asset format resolvers.
        /// </summary>
        public AssetResolver()
        {
            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Png,
                p => new AssetFile<Texture2DAssetDefinition>(p),
                () => new Texture2DAssetDefinition());

            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Jpeg,
                p => new AssetFile<Texture2DAssetDefinition>(p),
                () => new Texture2DAssetDefinition());

            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Wav,
                p => new AssetFile<SoundAssetDefinition>(p),
                () => new SoundAssetDefinition());

            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Ogg,
                p => new AssetFile<MusicAssetDefinition>(p),
                () => new MusicAssetDefinition());
        }

        /// <summary>
        /// Registers support for an asset format.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// Type of the asset definition.
        /// </typeparam>
        /// <param name="match">
        /// Function that determines whether a stream matches the asset format.
        /// </param>
        /// <param name="fileFactory">
        /// Function that creates the corresponding asset file.
        /// </param>
        /// <param name="definitionFactory">
        /// Function that creates the corresponding asset definition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when an argument is <see langword="null"/>.
        /// </exception>
        public void Register<TDefinition>(
            Func<Stream, bool> match,
            Func<string, AssetFile<TDefinition>> fileFactory,
            Func<TDefinition> definitionFactory)
            where TDefinition : class, IAssetDefinition
        {
            ArgumentNullException.ThrowIfNull(match);
            ArgumentNullException.ThrowIfNull(fileFactory);
            ArgumentNullException.ThrowIfNull(definitionFactory);

            _entries.Add(new ResolverEntry
            {
                DefinitionType = typeof(TDefinition),
                Match = match,
                FileFactory = fileFactory,
                DefinitionFactory = definitionFactory
            });
        }

        /// <summary>
        /// Resolves an asset file from the specified path and stream.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// Type of the asset definition.
        /// </typeparam>
        /// <param name="relativePath">
        /// Relative path of the asset inside the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <returns>
        /// The strongly typed asset file if the format is recognized;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> or <paramref name="stream"/>
        /// is <see langword="null"/>.
        /// </exception>
        public AssetFile<TDefinition>? Resolve<TDefinition>(
            string relativePath,
            Stream stream)
            where TDefinition : class, IAssetDefinition
        {
            ArgumentNullException.ThrowIfNull(relativePath);
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                if (entry.DefinitionType != typeof(TDefinition))
                    continue;

                long position = stream.CanSeek ? stream.Position : 0;

                bool matches = entry.Match(stream);

                if (stream.CanSeek)
                    stream.Position = position;

                if (matches)
                    return (AssetFile<TDefinition>)entry.FileFactory(relativePath);
            }

            return null;
        }

        /// <summary>
        /// Creates an asset file reference from the specified path and stream.
        /// </summary>
        /// <param name="relativePath">
        /// Relative path of the asset inside the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <returns>
        /// The asset file reference if the asset format is recognized;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> or <paramref name="stream"/>
        /// is <see langword="null"/>.
        /// </exception>
        public IAssetFile? Resolve(
            string relativePath,
            Stream stream)
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
                    return entry.FileFactory(relativePath);
            }

            return null;
        }

        /// <summary>
        /// Gets the asset definition for the specified asset file.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// Type of the asset definition.
        /// </typeparam>
        /// <param name="file">
        /// Asset file.
        /// </param>
        /// <returns>
        /// The corresponding asset definition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        public TDefinition GetDefinition<TDefinition>(AssetFile<TDefinition> file)
            where TDefinition : class, IAssetDefinition
        {
            ArgumentNullException.ThrowIfNull(file);

            foreach (var entry in _entries)
            {
                if (entry.DefinitionType == typeof(TDefinition))
                    return (TDefinition)entry.DefinitionFactory();
            }

            throw new InvalidOperationException(
                $"No asset definition factory is registered for '{typeof(TDefinition).FullName}'.");
        }

        /// <summary>
        /// Gets the asset definition for the specified asset file.
        /// </summary>
        /// <param name="file">
        /// Asset file reference.
        /// </param>
        /// <returns>
        /// The corresponding asset definition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        public IAssetDefinition? GetDefinition(IAssetFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            foreach (var entry in _entries)
            {
                if (entry.DefinitionType != file.AssetType)
                    continue;

                return entry.DefinitionFactory();
            }

            return null;
        }
    }
}