using System;

namespace Sachssoft.Sasogine.Audio
{
    public interface ISoundPlayer
    {

        void Play();

        void Stop();

        void Pause();

        void Resume();

        float Volume { get; set; }

        float Pitch { get; set; }

        bool IsPlaying { get; }

    }
}
