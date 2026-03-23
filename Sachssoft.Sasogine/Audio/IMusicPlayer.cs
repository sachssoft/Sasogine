using System;

namespace Sachssoft.Sasogine.Audio
{
    public interface IMusicPlayer
    {

        void Play();

        void Stop();

        void Pause();

        void Resume();

        float Volume { get; set; }

        float Pitch { get; set; }

        bool IsLooping { get; set; }

        TimeSpan StartOffset { get; set; }

        bool IsPlaying { get; }

        double Position { get; }

    }
}
