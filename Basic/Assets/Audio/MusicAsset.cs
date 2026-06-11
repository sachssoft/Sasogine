using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio
{
    public class MusicAsset : AssetBase<IMusicPlayer, MusicAssetDefinition>
    {
        protected override MusicAssetDefinition ResolveDefinition()
        {
            return new MusicAssetDefinition();
        }

        protected override IMusicPlayer? Build(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            try
            {
                IMusicPlayer? instance = null;

                switch (Definition.FormatType)
                {
                    case MusicFormatType.Auto:

                        switch (AudioDetection.DetectFormat(stream))
                        {
                            case AudioFormatType.Mp3:
                                instance = new Mp3StreamPlayer(stream);
                                break;
                            case AudioFormatType.Ogg:
                                instance = new OggStreamPlayer(stream);
                                break;
                        }
                        break;

                    case MusicFormatType.Ogg:
                        instance = new OggStreamPlayer(stream);
                        break;

                    case MusicFormatType.Mp3:
                        instance = new Mp3StreamPlayer(stream);
                        break;
                }

                if (instance != null)
                {
                    instance.Volume = Definition.Volume;
                    instance.Pitch = Definition.Pitch;
                    instance.StartOffset = Definition.StartOffset;
                    instance.IsLooping = Definition.IsLooping;
                    return instance;
                }

                throw new FormatException("Unsupported audio format. Only WAV and OGG are supported for sounds.");
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }
    }
}