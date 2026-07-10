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
    public static class AssetDetectionRegistry
    {
        private sealed class Entry
        {
            public required Func<Stream, bool> DetectionMatch { get; init; }

            public required Func<Stream, CancellationToken, ValueTask<bool>> DetectionMatchAsync { get; init; }

            public required Func<string, Stream, IAssetDefinition> Factory { get; init; }
        }


        private static readonly List<Entry> _entries = new();


        static AssetDetectionRegistry()
        {
            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Jpeg,
                async (s, ct) =>
                    await ImageDetection.DetectFormatAsync(s, ct) == ImageFormatType.Jpeg,
                (p, s) => new Texture2DAssetDefinition
                {
                    File = new AssetFile<Texture2DAsset>(p)
                });


            Register(
                s => ImageDetection.DetectFormat(s) == ImageFormatType.Png,
                async (s, ct) =>
                    await ImageDetection.DetectFormatAsync(s, ct) == ImageFormatType.Png,
                (p, s) => new Texture2DAssetDefinition
                {
                    File = new AssetFile<Texture2DAsset>(p)
                });


            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Wav,
                async (s, ct) =>
                    await AudioDetection.DetectFormatAsync(s, ct) == AudioFormatType.Wav,
                (p, s) => new SoundAssetDefinition
                {
                    File = new AssetFile<SoundAsset>(p)
                });


            Register(
                s => AudioDetection.DetectFormat(s) == AudioFormatType.Ogg,
                async (s, ct) =>
                    await AudioDetection.DetectFormatAsync(s, ct) == AudioFormatType.Ogg,
                (p, s) => new MusicAssetDefinition
                {
                    File = new AssetFile<MusicAsset>(p)
                });
        }


        public static void Register(
            Func<Stream, bool> detectionMatch,
            Func<Stream, CancellationToken, ValueTask<bool>> detectionMatchAsync,
            Func<string, Stream, IAssetDefinition> factory)
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


        public static bool Detect(
            string path,
            Stream stream,
            [MaybeNullWhen(false)] out IAssetDefinition assetDefinition)
        {
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                ResetPosition(stream);

                if (!entry.DetectionMatch(stream))
                    continue;

                ResetPosition(stream);

                assetDefinition = entry.Factory(path, stream);
                return true;
            }

            ResetPosition(stream);

            assetDefinition = null;
            return false;
        }


        public static async ValueTask<IAssetDefinition?> DetectAsync(
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