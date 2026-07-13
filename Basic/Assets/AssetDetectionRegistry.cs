using Sachssoft.Sasogine.Assets.Audio;
using Sachssoft.Sasogine.Assets.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Provides a registry for detecting asset types from streams and creating
    /// strongly typed asset file references.
    /// </summary>
    public static class AssetDetectionRegistry
    {
        private sealed class Entry
        {
            public required Func<Stream, bool> DetectionMatch { get; init; }

            public required Func<Stream, CancellationToken, ValueTask<bool>> DetectionMatchAsync { get; init; }

            public required Func<string, Stream, IAssetFile> Factory { get; init; }
        }


        private static readonly List<Entry> _entries = new();


        static AssetDetectionRegistry()
        {
            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Jpeg,
                async (s, ct) =>
                    await ImageDetection.DetectFormatAsync(s, ct) == ImageFormatType.Jpeg,
                (p, s) => new AssetFile<Texture2DAssetDefinition>(p));


            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Png,
                async (s, ct) =>
                    await ImageDetection.DetectFormatAsync(s, ct) == ImageFormatType.Png,
                (p, s) => new AssetFile<Texture2DAssetDefinition>(p));


            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Wav,
                async (s, ct) =>
                    await AudioDetection.DetectFormatAsync(s, ct) == AudioFormatType.Wav,
                (p, s) => new AssetFile<SoundAssetDefinition>(p));


            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Ogg,
                async (s, ct) =>
                    await AudioDetection.DetectFormatAsync(s, ct) == AudioFormatType.Ogg,
                (p, s) => new AssetFile<MusicAssetDefinition>(p));
        }


        /// <summary>
        /// Registers a new asset detection rule.
        /// </summary>
        /// <param name="detectionMatch">
        /// Function used to determine whether the stream matches the asset type.
        /// </param>
        /// <param name="detectionMatchAsync">
        /// Asynchronous function used to determine whether the stream matches the asset type.
        /// </param>
        /// <param name="factory">
        /// Function used to create the matching asset file reference.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="detectionMatch"/>, <paramref name="detectionMatchAsync"/>,
        /// or <paramref name="factory"/> is null.
        /// </exception>
        public static void Register(
            Func<Stream, bool> detectionMatch,
            Func<Stream, CancellationToken, ValueTask<bool>> detectionMatchAsync,
            Func<string, Stream, IAssetFile> factory)
        {
            ArgumentNullException.ThrowIfNull(detectionMatch);
            ArgumentNullException.ThrowIfNull(detectionMatchAsync);
            ArgumentNullException.ThrowIfNull(factory);

            _entries.Add(new Entry
            {
                DetectionMatch = detectionMatch,
                DetectionMatchAsync = detectionMatchAsync,
                Factory = factory
            });
        }


        /// <summary>
        /// Detects the asset type from the specified stream and creates a matching
        /// strongly typed asset file reference.
        /// </summary>
        /// <param name="path">
        /// Relative path of the asset inside the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <param name="assetFile">
        /// The detected asset file reference if detection succeeds; otherwise,
        /// <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching asset type was detected; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static bool Detect(
            string path,
            Stream stream,
            [MaybeNullWhen(false)] out IAssetFile assetFile)
        {
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                ResetPosition(stream);

                if (!entry.DetectionMatch(stream))
                    continue;

                ResetPosition(stream);

                assetFile = entry.Factory(path, stream);
                return true;
            }

            ResetPosition(stream);

            assetFile = null;
            return false;
        }


        /// <summary>
        /// Asynchronously detects the asset type from the specified stream and creates
        /// a matching strongly typed asset file reference.
        /// </summary>
        /// <param name="path">
        /// Relative path of the asset inside the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the detection operation.
        /// </param>
        /// <returns>
        /// The detected asset file reference if a matching asset type was found;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static async ValueTask<IAssetFile?> DetectAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                ResetPosition(stream);

                if (!await entry.DetectionMatchAsync(stream, cancellationToken))
                    continue;

                ResetPosition(stream);

                return entry.Factory(path, stream);
            }

            ResetPosition(stream);

            return null;
        }


        private static void ResetPosition(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
        }
    }
}