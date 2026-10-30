using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Represents a mutable time block, independently of its visual presentation.</summary>
public sealed class TimelineItem : Element
{
    /// <summary>Identifies changes to the optional block color.</summary>
    private static readonly PropertyChangedEventArgs ColorArgs = new(nameof(Color));

    /// <summary>Identifies changes to the block length.</summary>
    private static readonly PropertyChangedEventArgs DurationArgs = new(nameof(Duration));

    /// <summary>Identifies changes to the computed end time.</summary>
    private static readonly PropertyChangedEventArgs EndArgs = new(nameof(End));

    /// <summary>Identifies changes to editing permission.</summary>
    private static readonly PropertyChangedEventArgs IsLockedArgs = new(nameof(IsLocked));

    /// <summary>Identifies changes to the block start.</summary>
    private static readonly PropertyChangedEventArgs StartArgs = new(nameof(Start));

    /// <summary>Identifies changes to caller-owned data.</summary>
    private static readonly PropertyChangedEventArgs TagArgs = new(nameof(Tag));

    /// <summary>Identifies changes to the caption.</summary>
    private static readonly PropertyChangedEventArgs TitleArgs = new(nameof(Title));

    /// <summary>Stores the optional local block color.</summary>
    private Color? _color;

    /// <summary>Stores whether interactive editing is disabled.</summary>
    private bool _isLocked;

    /// <summary>Stores the positive block length in seconds.</summary>
    private double _duration = 1;

    /// <summary>Stores the nonnegative start time in seconds.</summary>
    private double _start;

    /// <summary>Stores application data without interpreting it.</summary>
    private object? _tag;

    /// <summary>Stores the displayed caption.</summary>
    private string _title = string.Empty;

    /// <summary>Gets or sets the optional fill color; null uses the timeline's styled color.</summary>
    public Color? Color
    {
        get => _color;
        set => SetNotify(ref _color, value, ColorArgs);
    }

    /// <summary>Gets or sets the positive finite block length in seconds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The resulting time range is invalid.</exception>
    public double Duration
    {
        get => _duration;
        set => SetRange(_start, value);
    }

    /// <summary>Gets the exclusive end of the block in seconds.</summary>
    public double End => _start + _duration;

    /// <summary>Gets or sets whether the block can be moved or resized interactively.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        set => SetNotify(ref _isLocked, value, IsLockedArgs);
    }

    /// <summary>Gets or sets the nonnegative finite start time in seconds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The resulting time range is invalid.</exception>
    public double Start
    {
        get => _start;
        set => SetRange(value, _duration);
    }

    /// <summary>Gets or sets optional application data, such as an animation definition.</summary>
    public object? Tag
    {
        get => _tag;
        set => SetNotify(ref _tag, value, TagArgs);
    }

    /// <summary>Gets or sets the non-null caption drawn inside the block.</summary>
    /// <exception cref="ArgumentNullException">The caption is null.</exception>
    public string Title
    {
        get => _title;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetNotify(ref _title, value, TitleArgs);
        }
    }

    /// <summary>Updates start and duration together before raising change notifications.</summary>
    /// <param name="start">The finite nonnegative start time in seconds.</param>
    /// <param name="duration">The finite positive duration in seconds.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range is negative, empty, or nonfinite.</exception>
    public void SetRange(double start, double duration)
    {
        if (!double.IsFinite(start) || start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (!double.IsFinite(duration) || duration <= 0 || !double.IsFinite(start + duration) || start + duration <= start)
            throw new ArgumentOutOfRangeException(nameof(duration));

        double previousEnd = End;
        bool startChanged = _start != start;
        bool durationChanged = _duration != duration;
        _start = start;
        _duration = duration;
        if (startChanged) NotifyPropertyChanged(StartArgs);
        if (durationChanged) NotifyPropertyChanged(DurationArgs);
        if (previousEnd != End) NotifyPropertyChanged(EndArgs);
    }
}
