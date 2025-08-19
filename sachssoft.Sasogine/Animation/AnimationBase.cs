using Microsoft.Xna.Framework;
using System;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Animation;

public abstract class AnimationBase : GameObject
{
    private Vector2 _start_position;
    private float _start_rotation;
    private bool _pause;
    private float _speed = 10f;
    private int _duration = 100;
    private bool _infinite = true;
    private int _delay = 0;
    private long _timer_ticks;
    private TimeSpan _start_timer;
    private TimeSpan _end_timer;

    public void Start(Vector2 position, float rotation)
    {
        _start_position = position;
        _start_rotation = rotation;

        StartTimer();
        OnStart();
    }

    protected virtual void OnStart() { }

    public void Reset()
    {
        StartTimer();
        OnReset();
    }

    protected virtual void OnReset() { }

    private void StartTimer()
    {
        _timer_ticks = Environment.TickCount64;
        _start_timer = new TimeSpan(_timer_ticks) + TimeSpan.FromMilliseconds(Delay);
        _end_timer = new TimeSpan(_timer_ticks) + TimeSpan.FromMilliseconds(Delay + Duration);
    }

    protected Vector2 StartPosition => _start_position;

    protected float StartRotation => _start_rotation;

    public void Pause()
    {
        _pause = !_pause;
    }

    private bool AllowUpdate()
    {
        _timer_ticks = Environment.TickCount64;

        // Überprüfe, ob die Startzeit erreicht ist und, wenn nicht, ob die Dauer überschritten ist
        if (_timer_ticks >= _start_timer.Ticks)
        {
            if (_infinite)
                return true;

            return _timer_ticks <= _end_timer.Ticks;
        }

        return false;
    }

    public Vector2 AddPosition(float elapsed_time)
    {
        // Position nur aktualisieren, wenn nicht pausiert und die Zeit erreicht ist
        if (!_pause && AllowUpdate())
        {
            return AddPositionOverride(elapsed_time);
        }
        else
        {
            return _start_position;
        }
    }

    protected virtual Vector2 AddPositionOverride(float elapsed_time)
    {
        // Hier überschreiben, um die Logik für die Position zu definieren
        return Vector2.Zero;
    }

    public float AddRotationDegree(float elapsed_time)
    {
        // Rotation nur aktualisieren, wenn nicht pausiert und die Zeit erreicht ist
        if (!_pause && AllowUpdate())
        {
            return AddRotationDegreeOverride(elapsed_time);
        }
        else
        {
            return _start_rotation;
        }
    }

    protected virtual float AddRotationDegreeOverride(float elapsed_time)
    {
        // Hier überschreiben, um die Logik für die Rotation zu definieren
        return 0f;
    }

    // Geschwindigkeit
    public virtual float Speed
    {
        get => _speed;
        set => RaiseAndSetIfChanged(ref _speed, float.Round(value, 2));
    }

    // Dauer der Laufzeit (nicht gültig, wenn Infinite = true)
    public virtual int Duration
    {
        get => _duration;
        set => RaiseAndSetIfChanged(ref _duration, value);
    }

    // Unendliche Laufzeit
    public virtual bool Infinite
    {
        get => _infinite;
        set => RaiseAndSetIfChanged(ref _infinite, value);
    }

    // Startverzögerung
    public virtual int Delay
    {
        get => _delay;
        set => RaiseAndSetIfChanged(ref _delay, value);
    }

    // Berechnet den Fortschritt der Animation (0 bis 1)
    protected float GetProgress()
    {
        long now = Environment.TickCount64;

        // Wenn die Startzeit noch nicht erreicht ist
        if (now < _start_timer.Ticks)
            return 0f;

        // Wenn die Animation unendlich ist oder die Endzeit überschritten wurde
        if (_infinite || now > _end_timer.Ticks)
            return 1f;

        // Berechnet den Prozentsatz der Animation basierend auf der verstrichenen Zeit
        return Math.Clamp(
            (float)(now - _start_timer.Ticks) / (float)(_end_timer.Ticks - _start_timer.Ticks),
            0f, 1f
        );
    }
}
