using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio
{
    public class MusicAsset : AssetBase2<IMusicPlayer, IMusicAssetDefinition>
    {
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

                        switch (AudioHelpers.DetectFormat(stream))
                        {
                            case AudioHelpers.AudioFormatType.Mp3:
                                instance = new Mp3StreamPlayer(stream);
                                break;
                            case AudioHelpers.AudioFormatType.Ogg:
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