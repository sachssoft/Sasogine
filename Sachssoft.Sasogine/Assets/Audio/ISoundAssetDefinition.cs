namespace Sachssoft.Sasogine.Assets.Audio
{
    public interface ISoundAssetDefinition : IAssetDefinition
    {
        SoundFormatType FormatType { get; set; }

        float Volume { get; set; }

        bool IsLooping { get; set; }

        float Pitch { get; set; }

        SoundCategory Category { get; set; }

    }
}
