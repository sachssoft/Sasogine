using System;

namespace Sachssoft.Sasogine.Assets.Audio
{
    public class MusicAssetDefinition : AssetDefinitionBase<MusicAsset>
    {
        /// <summary>Format of the music: Auto, OGG, MP3</summary>
        public MusicFormatType FormatType { get; set; }

        /// <summary>Volume (0.0 - 1.0)</summary>
        public float Volume { get; set; }

        /// <summary>Should the music loop?</summary>
        public bool IsLooping { get; set; }

        /// <summary>Playback speed / pitch multiplier (1.0 = normal speed)</summary>
        public float Pitch { get; set; }

        /// <summary>Start offset in seconds (skip first X seconds)</summary>
        public TimeSpan StartOffset { get; set; }

        // Für Referenz kann auch Kategorie gefiltert werden
        public MusicCategory Category { get; set; }
    }

}
