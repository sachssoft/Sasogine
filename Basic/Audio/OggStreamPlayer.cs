using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Audio
{
    public class OggStreamPlayer : AudioPlayerBase, IMusicPlayer, ISoundPlayer
    {
        private DynamicSoundEffectInstance? _output;
        private VorbisReader? _decoder;

        private const int BUFFER_SAMPLES = 8192; // größeres Puffer für glatteres Audio
        private readonly float[] _sampleBuffer;
        private readonly byte[] _byteBuffer;

        private float _volume = 1f;
        private float _pitch = 1f;
        private bool _isLooping = false;

        public OggStreamPlayer(Stream stream) : base(stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Stream cannot be null or empty", nameof(stream));

            _sampleBuffer = new float[BUFFER_SAMPLES];
            _byteBuffer = new byte[BUFFER_SAMPLES * 2];
        }

        public override float Volume
        {
            get => _volume;
            set
            {
                _volume = float.Clamp(value, 0f, 1f);
                if (_output != null) _output.Volume = _volume;
            }
        }

        public override float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = float.Clamp(value, 0.5f, 2f);
                if (_output != null)
                    _output.Pitch = _pitch - 1f;
            }
        }

        public override bool IsLooping
        {
            get => _isLooping;
            set
            {
                _isLooping = value;
                if (_output != null) _output.IsLooped = _isLooping;
            }
        }

        public override bool IsPlaying => _output?.State == SoundState.Playing;

        public override double Position => _decoder?.TimePosition.TotalSeconds ?? 0;

        public override void Play()
        {
            Stop();

            _decoder = new VorbisReader(Stream, false);

            // StartOffset anwenden
            if (StartOffset > TimeSpan.Zero)
                _decoder.TimePosition = StartOffset;

            _output = new DynamicSoundEffectInstance(
                _decoder.SampleRate,
                _decoder.Channels == 2 ? AudioChannels.Stereo : AudioChannels.Mono);

            _output.Volume = _volume;
            _output.Pitch = _pitch - 1f;
            _output.IsLooped = _isLooping;

            _output.BufferNeeded += OnBufferNeeded;
            _output.Play();
        }

        private void OnBufferNeeded(object? sender, EventArgs e)
        {
            if (_decoder == null || _output == null) return;

            int read = _decoder.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);

            if (read <= 0)
            {
                if (_isLooping)
                {
                    _decoder.SamplePosition = 0;
                    read = _decoder.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);
                }
                else
                {
                    Stop();
                    return;
                }
            }

            int bytes = ConvertSamples(_sampleBuffer, read);
            _output.SubmitBuffer(_byteBuffer, 0, bytes);
        }

        private int ConvertSamples(float[] samples, int count)
        {
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                short s = (short)(float.Clamp(samples[i], -1f, 1f) * short.MaxValue);
                _byteBuffer[index++] = (byte)(s & 0xff);
                _byteBuffer[index++] = (byte)(s >> 8);
            }
            return index;
        }

        public override void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
                _output.BufferNeeded -= OnBufferNeeded;
                _output.Dispose();
                _output = null;
            }

            _decoder?.Dispose();
            _decoder = null;

            if (Stream.CanSeek)
                Stream.Position = 0; // bereit für erneutes Play
        }

        public override void Pause()
        {
            if (_output != null && _output.State == SoundState.Playing)
                _output.Pause();
        }

        public override void Resume()
        {
            if (_output != null && _output.State == SoundState.Paused)
                _output.Play();
        }
    }
}