using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Audio;

/// <summary>
/// Provides streaming playback of Ogg Vorbis audio using a
/// <see cref="VorbisReader"/> decoder and a
/// <see cref="DynamicSoundEffectInstance"/> output.
/// </summary>
public class OggStreamPlayer :
    AudioPlayerBase,
    IMusicPlayer,
    ISoundPlayer
{
    private const int BufferSamples = 8192;

    private readonly float[] _sampleBuffer;
    private readonly byte[] _byteBuffer;

    private DynamicSoundEffectInstance? _output;
    private VorbisReader? _decoder;

    private float _volume = 1f;
    private float _pitch = 1f;
    private bool _isLooping;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggStreamPlayer"/> class.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded Ogg Vorbis audio data.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is empty.
    /// </exception>
    public OggStreamPlayer(Stream stream)
        : base(stream)
    {
        if (stream.CanSeek && stream.Length == 0)
        {
            throw new ArgumentException(
                "Stream cannot be empty.",
                nameof(stream));
        }

        _sampleBuffer = new float[BufferSamples];
        _byteBuffer = new byte[BufferSamples * sizeof(short)];
    }

    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    /// <remarks>
    /// The value is clamped to the range <c>0</c> to <c>1</c>.
    /// </remarks>
    public override float Volume
    {
        get => _volume;
        set
        {
            _volume = float.Clamp(value, 0f, 1f);

            if (_output != null)
                _output.Volume = _volume;
        }
    }

    /// <summary>
    /// Gets or sets the playback pitch.
    /// </summary>
    /// <remarks>
    /// The value is clamped to the range <c>0.5</c> to <c>2</c>.
    /// A value of <c>1</c> represents the normal pitch.
    /// </remarks>
    public override float Pitch
    {
        get => _pitch;
        set
        {
            _pitch = float.Clamp(value, 0.5f, 2f);

            if (_output != null)
                _output.Pitch = ToOutputPitch(_pitch);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether playback restarts
    /// after reaching the end of the stream.
    /// </summary>
    public override bool IsLooping
    {
        get => _isLooping;
        set => _isLooping = value;
    }

    /// <summary>
    /// Gets a value indicating whether audio is currently playing.
    /// </summary>
    public override bool IsPlaying =>
        _output?.State == SoundState.Playing;

    /// <summary>
    /// Gets the current playback position, in seconds.
    /// </summary>
    public override double Position =>
        _decoder?.TimePosition.TotalSeconds ?? 0d;

    /// <summary>
    /// Starts playback from the configured
    /// <see cref="AudioPlayerBase.StartOffset"/>.
    /// </summary>
    public override void Play()
    {
        Stop();

        CreateDecoder();

        _output = new DynamicSoundEffectInstance(
            _decoder!.SampleRate,
            _decoder.Channels == 2
                ? AudioChannels.Stereo
                : AudioChannels.Mono);

        _output.Volume = _volume;
        _output.Pitch = ToOutputPitch(_pitch);
        _output.BufferNeeded += OnBufferNeeded;

        FillBuffer();

        _output.Play();
    }

    /// <summary>
    /// Stops playback and releases the active decoder and audio output.
    /// </summary>
    public override void Stop()
    {
        if (_output != null)
        {
            _output.BufferNeeded -= OnBufferNeeded;

            _output.Stop();
            _output.Dispose();
            _output = null;
        }

        _decoder?.Dispose();
        _decoder = null;

        if (Stream.CanSeek)
            Stream.Position = 0;
    }

    /// <summary>
    /// Pauses playback when audio is currently playing.
    /// </summary>
    public override void Pause()
    {
        if (_output?.State == SoundState.Playing)
            _output.Pause();
    }

    /// <summary>
    /// Resumes playback when audio is currently paused.
    /// </summary>
    public override void Resume()
    {
        if (_output?.State == SoundState.Paused)
            _output.Resume();
    }

    private void OnBufferNeeded(
        object? sender,
        EventArgs e)
    {
        FillBuffer();
    }

    private void FillBuffer()
    {
        if (_decoder == null || _output == null)
            return;

        int read = _decoder.ReadSamples(
            _sampleBuffer,
            0,
            _sampleBuffer.Length);

        if (read <= 0)
        {
            if (!_isLooping)
            {
                Stop();
                return;
            }

            RestartDecoder();

            read = _decoder!.ReadSamples(
                _sampleBuffer,
                0,
                _sampleBuffer.Length);

            if (read <= 0)
            {
                Stop();
                return;
            }
        }

        int byteCount =
            ConvertSamples(
                _sampleBuffer,
                read);

        _output.SubmitBuffer(
            _byteBuffer,
            0,
            byteCount);
    }

    private void CreateDecoder()
    {
        if (Stream.CanSeek)
            Stream.Position = 0;

        _decoder?.Dispose();

        _decoder = new VorbisReader(
            Stream,
            closeOnDispose: false);

        ApplyStartOffset();
    }

    private void RestartDecoder()
    {
        if (!Stream.CanSeek)
        {
            throw new InvalidOperationException(
                "The Ogg stream must be seekable to support looping.");
        }

        CreateDecoder();
    }

    private void ApplyStartOffset()
    {
        if (_decoder == null ||
            StartOffset <= TimeSpan.Zero)
        {
            return;
        }

        if (StartOffset >= _decoder.TotalTime)
        {
            _decoder.TimePosition =
                _decoder.TotalTime;

            return;
        }

        _decoder.TimePosition =
            StartOffset;
    }

    private int ConvertSamples(
        float[] samples,
        int count)
    {
        int index = 0;

        for (int i = 0; i < count; i++)
        {
            short sample = (short)(
                float.Clamp(
                    samples[i],
                    -1f,
                    1f) *
                short.MaxValue);

            _byteBuffer[index++] =
                (byte)(sample & 0xFF);

            _byteBuffer[index++] =
                (byte)((sample >> 8) & 0xFF);
        }

        return index;
    }

    private static float ToOutputPitch(float pitch)
    {
        return float.Clamp(
            pitch - 1f,
            -1f,
            1f);
    }
}