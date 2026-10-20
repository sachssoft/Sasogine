using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Indeterminate loading indicator animated from frame time without a background timer.</summary>
public class Spinner : Control
{
    private ChangeTracker<bool> _active;
    private ChangeTracker<float> _speed;
    private double _phase;
    /// <summary>Creates a loading spinner.</summary>
    public Spinner() : base()
    {
    }

    /// <summary>Gets or sets whether the indicator advances.</summary>
    public bool IsSpinning { get => ConfigIsSpinning; set => ConfigIsSpinning = value; }
    /// <summary>Gets normalized rotation phase.</summary>
    public float Phase => (float)_phase;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Spinner";

    internal override void Prepare()
    {
        base.Prepare();
        _active.EnsureChange(ConfigIsSpinning);
        _speed.EnsureChange(ConfigAnimationSpeed);
        if (!float.IsFinite(_speed.NewValue) || _speed.NewValue < 0)
            throw new InvalidOperationException("Invalid animation speed.");
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(MathF.Min(32, available.Width), MathF.Min(32, available.Height));
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_active.NewValue && IsAvailable())
            _phase = (_phase + elapsed.TotalSeconds * _speed.NewValue) % 1;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        Vector2 center = new(Bounds.Width / 2, Bounds.Height / 2);
        float radius = MathF.Max(0, MathF.Min(ContentBounds.Width, ContentBounds.Height) / 2 - 2);
        for (int i = 0; i < 12; i++)
        {
            float angle = (i / 12f + (float)_phase) * MathF.Tau;
            Vector2 direction = new(MathF.Cos(angle), MathF.Sin(angle));
            context.DrawLine(center + direction * radius * .55f, center + direction * radius, (IsEnabled ? ConfigTextColor : ConfigDisabledTextColor) * (.18f + .82f * i / 11), MathF.Max(1, radius * .16f));
        }
    }

    private StyleProperty<bool> _styleIsSpinning = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsSpinningChangedArgs = new(nameof(IsSpinning));
    internal bool ConfigIsSpinning
    {
        get => _styleIsSpinning.Value;
        set
        {
            if (_styleIsSpinning.SetLocal(value))
                NotifyPropertyChanged(IsSpinningChangedArgs);
        }
    }

    private StyleProperty<float> _styleAnimationSpeed = new(1f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs AnimationSpeedChangedArgs = new(nameof(AnimationSpeed));
    internal float ConfigAnimationSpeed
    {
        get => _styleAnimationSpeed.Value;
        set
        {
            if (_styleAnimationSpeed.SetLocal(value))
                NotifyPropertyChanged(AnimationSpeedChangedArgs);
        }
    }

    /// <summary>Gets or sets AnimationSpeed; a local assignment overrides the style.</summary>
    public float AnimationSpeed { get => ConfigAnimationSpeed; set => ConfigAnimationSpeed = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsSpinning":
                    if (_styleIsSpinning.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsSpinningChangedArgs);
                    break;
                case "AnimationSpeed":
                    if (_styleAnimationSpeed.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(AnimationSpeedChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsSpinning":
                return _styleIsSpinning.HasLocalValue;
            case "AnimationSpeed":
                return _styleAnimationSpeed.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsSpinning":
                if (_styleIsSpinning.ClearLocal())
                    NotifyPropertyChanged(IsSpinningChangedArgs);
                return;
            case "AnimationSpeed":
                if (_styleAnimationSpeed.ClearLocal())
                    NotifyPropertyChanged(AnimationSpeedChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleIsSpinning.BeginStyle(true))
            NotifyPropertyChanged(IsSpinningChangedArgs);
        if (_styleAnimationSpeed.BeginStyle(1f))
            NotifyPropertyChanged(AnimationSpeedChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsSpinning.CommitStyle())
            NotifyPropertyChanged(IsSpinningChangedArgs);
        if (_styleAnimationSpeed.CommitStyle())
            NotifyPropertyChanged(AnimationSpeedChangedArgs);
    }
}
