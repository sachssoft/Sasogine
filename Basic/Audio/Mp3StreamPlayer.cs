using Microsoft.Xna.Framework.Audio;
using NLayer;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Audio
{
    public class Mp3StreamPlayer : AudioPlayerBase, IMusicPlayer
    {
        private DynamicSoundEffectInstance? _output;
        private MpegFile? _decoder;

        private const int BUFFER_SAMPLES = 8192; // größeres Puffer für flüssigeres Audio
        private readonly float[] _sampleBuffer;
        private readonly byte[] _byteBuffer;

        private float _volume = 1f;
        private float _pitch = 1f;
        private bool _isLooping = false;

        public Mp3StreamPlayer(Stream stream) : base(stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentException("Stream cannot be null or empty", nameof(stream));

            _sampleBuffer = new float[BUFFER_SAMPLES];
            _byteBuffer = new byte[BUFFER_SAMPLES * 2];
        }

        // Properties
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
                _pitch = float.Clamp(value, 0.5f, 2f); // sicherer Bereich
                if (_output != null)
                    _output.Pitch = _pitch - 1f; // DynamicSoundEffectInstance: -1..1
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

        public override double Position => _decoder?.Time.TotalSeconds ?? 0;

        public override void Play()
        {
            Stop(); // alte Instanz beenden

            _decoder = new MpegFile(Stream);

            ApplyStartOffset();

            _output = new DynamicSoundEffectInstance(
                _decoder.SampleRate,
                _decoder.Channels == 2 ? AudioChannels.Stereo : AudioChannels.Mono);

            _output.Volume = _volume;
            _output.Pitch = _pitch - 1f;
            _output.IsLooped = _isLooping;
            _output.BufferNeeded += OnBufferNeeded;
            _output.Play();
        }

        private void ApplyStartOffset()
        {
            if (StartOffset <= TimeSpan.Zero || _decoder == null) return;

            if (!Stream.CanSeek)
                throw new InvalidOperationException("MP3 stream is not seekable, cannot apply StartOffset");

            Stream.Position = 0;
            _decoder = new MpegFile(Stream);

            long targetSample = (long)(StartOffset.TotalSeconds * _decoder.SampleRate);

            float[] tempBuffer = new float[BUFFER_SAMPLES];
            long skipped = 0;
            while (skipped < targetSample)
            {
                int toRead = (int)float.Min(BUFFER_SAMPLES, targetSample - skipped);
                int read = _decoder.ReadSamples(tempBuffer, 0, toRead);
                if (read <= 0) break;
                skipped += read;
            }
        }

        private void OnBufferNeeded(object? sender, EventArgs e)
        {
            if (_decoder == null || _output == null) return;

            int read = _decoder.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);

            if (read <= 0)
            {
                if (_isLooping)
                {
                    if (!Stream.CanSeek)
                        throw new InvalidOperationException("Stream ist nicht seekbar, Looping nicht möglich.");

                    Stream.Position = 0;
                    _decoder = new MpegFile(Stream);
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
                Stream.Position = 0; // für erneutes Play vorbereiten
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