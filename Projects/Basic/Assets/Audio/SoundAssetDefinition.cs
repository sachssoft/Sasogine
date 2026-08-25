namespace Sachssoft.Sasogine.Assets.Audio
{
    public class SoundAssetDefinition : AssetDefinitionBase<SoundAsset>
    {
        public SoundFormatType FormatType { get; set; }

        public float Volume { get; set; }

        public bool IsLooping { get; set; }

        public float Pitch { get; set; }

        public SoundCategory Category { get; set; }

    }
}
