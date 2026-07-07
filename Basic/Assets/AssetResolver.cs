using System;
using System.Collections.Generic;
using System.IO;
using Sachssoft.Sasogine.Assets.Audio;
using Sachssoft.Sasogine.Assets.Graphics;

namespace Sachssoft.Sasogine.Assets
{
    public class AssetResolver : IAssetResolverProvider
    {
        private readonly List<ResolverEntry> _entries = new();

        private sealed class ResolverEntry
        {
            public Func<Stream, bool> Match { get; init; } = default!;
            public Func<Stream, IAsset> Factory { get; init; } = default!;
        }

        public AssetResolver()
        {
            Register(s => ImageDetection.DetectFormat(s) == ImageFormatType.Png, s => new Texture2DAsset());
            Register(s => ImageDetection.DetectFormat(s) == ImageFormatType.Jpeg, s => new Texture2DAsset());
            //Register(s => ImageDetection.DetectFormat(s) == ImageFormatType.Bmp, s => new Texture2DAsset());
            //Register(s => ImageDetection.DetectFormat(s) == ImageFormatType.Dds, s => new Texture2DAsset());
            //Register(s => ImageDetection.DetectFormat(s) == ImageFormatType.Tga, s => new Texture2DAsset());

            Register(s => AudioDetection.DetectFormat(s) == AudioFormatType.Wav, s => new SoundAsset());
            Register(s => AudioDetection.DetectFormat(s) == AudioFormatType.Ogg, s => new MusicAsset());
        }

        public void Register(Func<Stream, bool> match, Func<Stream, IAsset> factory)
        {
            ArgumentNullException.ThrowIfNull(match);
            ArgumentNullException.ThrowIfNull(factory);

            _entries.Add(new ResolverEntry
            {
                Match = match,
                Factory = factory
            });
        }

        public IAsset? Resolve(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            foreach (var entry in _entries)
            {
                long position = stream.CanSeek ? stream.Position : 0;

                bool matches = entry.Match(stream);

                if (stream.CanSeek)
                    stream.Position = position;

                if (matches)
                    return entry.Factory(stream);
            }

            return null;
        }
    }
}