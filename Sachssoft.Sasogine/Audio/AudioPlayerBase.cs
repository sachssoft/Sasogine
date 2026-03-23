using System;
using System.IO;

namespace Sachssoft.Sasogine.Audio
{
    public abstract class AudioPlayerBase
    {
        public AudioPlayerBase(Stream stream)
        {
            Stream = stream;
        }

        protected Stream Stream { get; }

        /// <summary>Stream abspielen</summary>
        public abstract void Play();

        /// <summary>Stoppt die Wiedergabe</summary>
        public abstract void Stop();

        /// <summary>Pausiert die Wiedergabe</summary>
        public abstract void Pause();

        /// <summary>Fortsetzen der Wiedergabe nach Pause</summary>
        public abstract void Resume();

        /// <summary>Aktuelle Lautstärke (0.0 - 1.0)</summary>
        public virtual float Volume { get; set; }

        /// <summary>Soll Musik geloopt werden?</summary>
        public virtual bool IsLooping { get; set; } = false;

        /// <summary>Playback speed / pitch multiplier (1.0 = normal)</summary>
        public virtual float Pitch { get; set; }

        /// <summary>Gibt an, ob gerade gespielt wird</summary>
        public abstract bool IsPlaying { get; }

        /// <summary>Start offset for playback</summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>Optional: aktuelle Position in Sekunden</summary>
        public abstract double Position { get; }
    }
}