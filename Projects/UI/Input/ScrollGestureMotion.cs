using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls.Input;

/// <summary>Tracks recent pointer motion and advances a scroll gesture after release.</summary>
internal struct ScrollGestureMotion
{
    private const float Deceleration = 10f;
    private const float MaxSpeed = 1600f;
    private const float MinReleaseSpeed = 45f;
    private const float StopSpeed = 10f;
    private const double MaxSampleGap = 0.1;

    private Vector2 _velocity;
    private double _sampleTime;

    /// <summary>Gets whether the released gesture still has motion.</summary>
    internal bool IsCoasting { get; private set; }

    /// <summary>Starts a gesture at the current workspace input time.</summary>
    internal void Begin(double time)
    {
        this = default;
        _sampleTime = time;
    }

    /// <summary>Returns the portion of this pointer step beyond the activation threshold.</summary>
    internal static Vector2 AfterThreshold(Vector2 total, Vector2 step, float threshold)
    {
        bool horizontal = MathF.Abs(total.X) >= MathF.Abs(total.Y);
        float current = horizontal ? total.X : total.Y;
        float delta = horizontal ? step.X : step.Y;
        float previous = current - delta;
        if (delta == 0 || MathF.Abs(previous) >= threshold)
            return step;

        float fractionBeforeThreshold = (MathF.CopySign(threshold, current) - previous) / delta;
        return step * (1f - Math.Clamp(fractionBeforeThreshold, 0f, 1f));
    }

    /// <summary>Samples the movement that the scroll range actually accepted.</summary>
    internal void Track(Vector2 applied, double time)
    {
        double seconds = time - _sampleTime;
        if (applied == Vector2.Zero || seconds <= 0)
            return;

        _sampleTime = time;
        if (seconds > MaxSampleGap)
        {
            _velocity = Vector2.Zero;
            return;
        }

        Vector2 sample = Vector2.Clamp(applied / (float)Math.Max(seconds, 1.0 / 240), new Vector2(-MaxSpeed), new Vector2(MaxSpeed));
        float blend = 1f - MathF.Exp(-32f * (float)seconds);
        _velocity = _velocity == Vector2.Zero || Vector2.Dot(_velocity, sample) < 0
            ? sample : Vector2.Lerp(_velocity, sample, blend);
    }

    /// <summary>Starts deceleration if the pointer was moving immediately before release.</summary>
    internal void Release(double time)
    {
        double pause = time - _sampleTime;
        _velocity *= MathF.Exp(-Deceleration * (float)Math.Max(0, pause));
        IsCoasting = pause <= MaxSampleGap && _velocity.LengthSquared() >= MinReleaseSpeed * MinReleaseSpeed;
        _sampleTime = time;
    }

    /// <summary>Integrates the deceleration up to the current workspace input time.</summary>
    internal bool TryAdvance(double time, out Vector2 movement)
    {
        movement = Vector2.Zero;
        double seconds = time - _sampleTime;
        if (seconds <= 0)
            return false;

        _sampleTime = time;
        if (seconds > MaxSampleGap)
        {
            this = default;
            return false;
        }

        float decay = MathF.Exp(-Deceleration * (float)seconds);
        movement = _velocity * ((1f - decay) / Deceleration);
        _velocity *= decay;
        IsCoasting = _velocity.LengthSquared() >= StopSpeed * StopSpeed;
        return true;
    }

    /// <summary>Stops motion on axes that reached a scroll boundary.</summary>
    internal void StopBlockedAxes(bool horizontalBlocked, bool verticalBlocked)
    {
        if (horizontalBlocked)
            _velocity.X = 0;
        if (verticalBlocked)
            _velocity.Y = 0;
        IsCoasting &= _velocity.LengthSquared() >= StopSpeed * StopSpeed;
    }
}
