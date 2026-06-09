using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio
{
    public class SoundAsset : AssetBase<ISoundPlayer, SoundAssetDefinition>
    {
        protected override SoundAssetDefinition ResolveDefinition()
        {
            return new SoundAssetDefinition();
        }

        protected override ISoundPlayer? Build(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                return null;

            try
            {
                ISoundPlayer? instance = null;

                switch (Definition.FormatType)
                {
                    case SoundFormatType.Auto:

                        switch (AudioHelpers.DetectFormat(stream))
                        {
                            case AudioHelpers.AudioFormatType.Wav:
                                instance = new WavPlayer(stream);
                                break;
                            case AudioHelpers.AudioFormatType.Ogg:
                                instance = new OggStreamPlayer(stream);
                                break;
                        }
                        break;

                    case SoundFormatType.Ogg:
                        instance = new OggStreamPlayer(stream);
                        break;

                    case SoundFormatType.Wav:
                        instance = new WavPlayer(stream);
                        break;
                }

                if (instance != null)
                {
                    instance.Volume = Definition.Volume;
                    instance.Pitch = Definition.Pitch;
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