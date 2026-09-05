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
    /// Provides a registry for detecting asset types from streams
    /// and creating corresponding asset file references.
    /// </summary>
    public static class AssetDetectionRegistry
    {
        private sealed class Entry
        {
            public required Func<Stream, bool> DetectionMatch { get; init; }

            public required Func<Stream, CancellationToken, ValueTask<bool>>
                DetectionMatchAsync
            { get; init; }

            public required Func<string, Stream, IAssetFile> Factory { get; init; }
        }

        private static readonly List<Entry> _entries = new();

        static AssetDetectionRegistry()
        {
            Register(
                stream =>
                    ImageDetection.DetectFormat(stream) ==
                    ImageFormatType.Jpeg,

                async (stream, cancellationToken) =>
                    await ImageDetection.DetectFormatAsync(
                        stream,
                        cancellationToken) ==
                    ImageFormatType.Jpeg,

                (path, stream) =>
                    new AssetFile<Texture2DAssetDefinition>(path));

            Register(
                stream =>
                    ImageDetection.DetectFormat(stream) ==
                    ImageFormatType.Png,

                async (stream, cancellationToken) =>
                    await ImageDetection.DetectFormatAsync(
                        stream,
                        cancellationToken) ==
                    ImageFormatType.Png,

                (path, stream) =>
                    new AssetFile<Texture2DAssetDefinition>(path));

            Register(
                stream =>
                    AudioDetection.DetectFormat(stream) ==
                    AudioFormatType.Wav,

                async (stream, cancellationToken) =>
                    await AudioDetection.DetectFormatAsync(
                        stream,
                        cancellationToken) ==
                    AudioFormatType.Wav,

                (path, stream) =>
                    new AssetFile<SoundAssetDefinition>(path));

            Register(
                stream =>
                    AudioDetection.DetectFormat(stream) ==
                    AudioFormatType.Ogg,

                async (stream, cancellationToken) =>
                    await AudioDetection.DetectFormatAsync(
                        stream,
                        cancellationToken) ==
                    AudioFormatType.Ogg,

                (path, stream) =>
                    new AssetFile<MusicAssetDefinition>(path));
        }

        /// <summary>
        /// Registers an asset detection rule.
        /// </summary>
        /// <param name="detectionMatch">
        /// The synchronous function used to determine whether a stream
        /// matches the registered asset type.
        /// </param>
        /// <param name="detectionMatchAsync">
        /// The asynchronous function used to determine whether a stream
        /// matches the registered asset type.
        /// </param>
        /// <param name="factory">
        /// The function used to create the corresponding asset file reference.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="detectionMatch"/>,
        /// <paramref name="detectionMatchAsync"/>, or
        /// <paramref name="factory"/> is <see langword="null"/>.
        /// </exception>
        public static void Register(
            Func<Stream, bool> detectionMatch,
            Func<Stream, CancellationToken, ValueTask<bool>> detectionMatchAsync,
            Func<string, Stream, IAssetFile> factory)
        {
            ArgumentNullException.ThrowIfNull(detectionMatch);
            ArgumentNullException.ThrowIfNull(detectionMatchAsync);
            ArgumentNullException.ThrowIfNull(factory);

            _entries.Add(
                new Entry
                {
                    DetectionMatch = detectionMatch,
                    DetectionMatchAsync = detectionMatchAsync,
                    Factory = factory
                });
        }

        /// <summary>
        /// Detects the asset type represented by the specified stream.
        /// </summary>
        /// <param name="path">
        /// The relative path associated with the asset.
        /// </param>
        /// <param name="stream">
        /// The stream containing the asset data.
        /// </param>
        /// <param name="assetFile">
        /// When this method returns <see langword="true"/>, contains the
        /// detected asset file reference; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching asset type was detected;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is <see langword="null"/>.
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

                assetFile = entry.Factory(
                    path,
                    stream);

                return true;
            }

            ResetPosition(stream);

            assetFile = null;
            return false;
        }

        /// <summary>
        /// Asynchronously detects the asset type represented by the
        /// specified stream.
        /// </summary>
        /// <param name="path">
        /// The relative path associated with the asset.
        /// </param>
        /// <param name="stream">
        /// The stream containing the asset data.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to cancel the detection operation.
        /// </param>
        /// <returns>
        /// The detected asset file reference, or <see langword="null"/>
        /// if no matching asset type was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is <see langword="null"/>.
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

                if (!await entry.DetectionMatchAsync(
                        stream,
                        cancellationToken)
                    .ConfigureAwait(false))
                {
                    continue;
                }

                ResetPosition(stream);

                return entry.Factory(
                    path,
                    stream);
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