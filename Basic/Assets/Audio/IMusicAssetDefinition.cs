using System;

namespace Sachssoft.Sasogine.Assets.Audio
{
    public interface IMusicAssetDefinition : IAssetDefinition
    {
        /// <summary>Format of the music: Auto, OGG, MP3</summary>
        MusicFormatType FormatType { get; set; }

        /// <summary>Volume (0.0 - 1.0)</summary>
        float Volume { get; set; }

        /// <summary>Should the music loop?</summary>
        bool IsLooping { get; set; }

        /// <summary>Playback speed / pitch multiplier (1.0 = normal speed)</summary>
        float Pitch { get; set; }

        /// <summary>Start offset in seconds (skip first X seconds)</summary>
        TimeSpan StartOffset { get; set; }

        // Für Referenz kann auch Kategorie gefiltert werden
        MusicCategory Category { get; set; }
    }

}
