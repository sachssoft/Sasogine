using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;
using System.IO;

namespace Sachssoft.Sasogine.Audio;

/// <summary>
/// Provides playback of WAV audio using a <see cref="SoundEffect"/>.
/// </summary>
/// <remarks>
/// WAV audio is loaded into memory when the player is created.
/// The reported playback position is estimated using a
/// <see cref="Stopwatch"/> and may not exactly match the underlying
/// audio device position.
/// </remarks>
public class WavPlayer : AudioPlayerBase, ISoundPlayer
{
    private readonly SoundEffect _soundEffect;

    private SoundEffectInstance? _instance;
    private Stopwatch? _stopwatch;

    private float _volume = 1f;
    private bool _isLooping;
    private float _pitch = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavPlayer"/> class.
    /// </summary>
    /// <param name="stream">
    /// The stream containing WAV audio data.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is too short to contain a valid WAV header.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the stream does not contain a valid RIFF/WAVE header.
    /// </exception>
    public WavPlayer(Stream stream)
        : base(stream)
    {
        if (!stream.CanSeek || stream.Length - stream.Position < 12)
        {
            throw new ArgumentException(
                "Stream is too short to contain a valid WAV file.",
                nameof(stream));
        }

        long originalPosition = stream.Position;

        Span<byte> header = stackalloc byte[12];

        try
        {
            int bytesRead = stream.Read(header);

            if (bytesRead < header.Length ||
                header[0] != (byte)'R' ||
                header[1] != (byte)'I' ||
                header[2] != (byte)'F' ||
                header[3] != (byte)'F' ||
                header[8] != (byte)'W' ||
                header[9] != (byte)'A' ||
                header[10] != (byte)'V' ||
                header[11] != (byte)'E')
            {
                throw new InvalidDataException(
                    "The provided stream is not a valid WAV file.");
            }
        }
        finally
        {
            stream.Position = originalPosition;
        }

        _soundEffect = SoundEffect.FromStream(stream);
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

            if (_instance != null)
                _instance.Volume = _volume;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether playback repeats
    /// after reaching the end of the sound.
    /// </summary>
    public override bool IsLooping
    {
        get => _isLooping;
        set
        {
            _isLooping = value;

            if (_instance != null)
                _instance.IsLooped = _isLooping;
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

            if (_instance != null)
                _instance.Pitch = ToOutputPitch(_pitch);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the sound is currently playing.
    /// </summary>
    public override bool IsPlaying =>
        _instance?.State == SoundState.Playing;

    /// <summary>
    /// Gets the approximate playback position, in seconds.
    /// </summary>
    /// <remarks>
    /// The position is estimated using a <see cref="Stopwatch"/>
    /// and may differ from the actual audio device position.
    /// </remarks>
    public override double Position =>
        _stopwatch?.Elapsed.TotalSeconds ?? 0d;

    /// <summary>
    /// Starts playback from the beginning of the sound.
    /// </summary>
    public override void Play()
    {
        Stop();

        _instance = _soundEffect.CreateInstance();

        _instance.Volume = _volume;
        _instance.Pitch = ToOutputPitch(_pitch);
        _instance.IsLooped = _isLooping;

        _instance.Play();

        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops playback and releases the active sound instance.
    /// </summary>
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

    /// <summary>
    /// Pauses playback when the sound is currently playing.
    /// </summary>
    public override void Pause()
    {
        if (_instance?.State != SoundState.Playing)
            return;

        _instance.Pause();
        _stopwatch?.Stop();
    }

    /// <summary>
    /// Resumes playback when the sound is currently paused.
    /// </summary>
    public override void Resume()
    {
        if (_instance?.State != SoundState.Paused)
            return;

        _instance.Resume();
        _stopwatch?.Start();
    }

    private static float ToOutputPitch(float pitch)
    {
        return float.Clamp(
            pitch - 1f,
            -1f,
            1f);
    }
}