using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;
using System.IO;

namespace Sachssoft.Sasogine.Audio
{
    public class WavPlayer : AudioPlayerBase, ISoundPlayer
    {
        private readonly SoundEffect _soundEffect;
        private SoundEffectInstance? _instance;
        private Stopwatch? _stopwatch;

        public WavPlayer(Stream stream) : base(stream)
        {
            if (stream == null || stream.Length < 12)
                throw new ArgumentException("Stream is null or too short to be a valid WAV.", nameof(stream));

            // Prüfen Magic Bytes: RIFF....WAVE
            long pos = stream.Position;
            byte[] header = new byte[12];
            stream.Read(header, 0, header.Length);
            stream.Position = pos;

            if (!(header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F' &&
                  header[8] == 'W' && header[9] == 'A' && header[10] == 'V' && header[11] == 'E'))
            {
                throw new InvalidDataException("The provided stream is not a valid WAV file.");
            }

            _soundEffect = SoundEffect.FromStream(stream);
        }

        private float _volume = 1f;
        public override float Volume
        {
            get => _volume;
            set
            {
                _volume = float.Clamp(value, 0f, 1f);
                if (_instance != null) _instance.Volume = _volume;
            }
        }

        private bool _isLooping = false;
        public override bool IsLooping
        {
            get => _isLooping;
            set
            {
                _isLooping = value;
                if (_instance != null) _instance.IsLooped = _isLooping;
            }
        }

        private float _pitch = 1f;
        public override float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = float.Clamp(value, 0.5f, 2f);
                if (_instance != null)
                    _instance.Pitch = _pitch - 1f;
            }
        }

        public override bool IsPlaying => _instance?.State == SoundState.Playing;

        /// <summary>
        /// Approximate playback position in seconds.
        /// Note: For WAV short sounds this is only an estimate using Stopwatch and may not be exact.
        /// </summary>
        public override double Position => _stopwatch?.Elapsed.TotalSeconds ?? 0;

        public override void Play()
        {
            Stop();

            _instance = _soundEffect.CreateInstance();
            _instance.Volume = _volume;
            _instance.Pitch = _pitch - 1f;
            _instance.IsLooped = _isLooping;
            _instance.Play();

            _stopwatch = Stopwatch.StartNew();
        }

        public override void Stop()
        {
            if (_instance != null)
            {
                _instance.Stop();
                _instance.Dispose();
                _instance = null;
            }
            _stopwatch?.Stop();
            _stopwatch = null;
        }

        public override void Pause()
        {
            if (_instance != null && _instance.State == SoundState.Playing)
            {
                _instance.Pause();
                _stopwatch?.Stop();
            }
        }

        public override void Resume()
        {
            if (_instance != null && _instance.State == SoundState.Paused)
            {
                _instance.Play();
                _stopwatch?.Start();
            }
        }
    }
}