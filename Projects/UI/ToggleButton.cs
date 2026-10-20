using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>A push button that retains its checked state.</summary>
public class ToggleButton : ButtonBase
{
    /// <summary>Initializes a new toggle button instance.</summary>
    public ToggleButton() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToggleButton";
    /// <summary>Gets or sets is checked.</summary>
    public bool IsChecked { get => ConfigIsChecked == true; set => SetChecked(value); }

    /// <summary>Occurs when checked changed.</summary>
    public event EventHandler? CheckedChanged;
    /// <summary>Performs set checked using the current control state.</summary>
    protected void SetChecked(bool? value)
    {
        bool? previous = ConfigIsChecked;
        ConfigIsChecked = value;
        if (previous != value)
        {
            OnCheckedChanged();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Performs on checked changed using the current control state.</summary>
    protected virtual void OnCheckedChanged()
    {
    }

    /// <summary>Performs change checked state using the current control state.</summary>
    protected virtual void ChangeCheckedState() => IsChecked = !IsChecked;
    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (!CanExecute)
            return;
        ChangeCheckedState();
        base.OnClick();
    }

    /// <summary>Gets selection state.</summary>
    protected string SelectionState => (ConfigIsChecked is null ? "Mixed" : IsChecked ? "Checked" : "Unchecked") + "." + (!IsAvailable() ? "Disabled" : IsHeld ? "Pressed" : IsPointerOver ? "Hover" : "Normal");

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds, SelectionState))
            context.FillRectangle(LocalBounds, IsChecked ? PressedBackground : Background);
    }

    private StyleProperty<bool?> _styleIsChecked = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsCheckedChangedArgs = new(nameof(IsChecked));
    internal bool? ConfigIsChecked
    {
        get => _styleIsChecked.Value;
        set
        {
            if (_styleIsChecked.SetLocal(value))
                NotifyPropertyChanged(IsCheckedChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsChecked":
                    if (_styleIsChecked.Apply(Converters.Convert<bool?>("NullableBoolean", entry.Value)))
                        NotifyPropertyChanged(IsCheckedChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsChecked":
                return _styleIsChecked.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsChecked":
                if (_styleIsChecked.ClearLocal())
                    NotifyPropertyChanged(IsCheckedChangedArgs);
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
        if (_styleIsChecked.BeginStyle(false))
            NotifyPropertyChanged(IsCheckedChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsChecked.CommitStyle())
            NotifyPropertyChanged(IsCheckedChangedArgs);
    }
}
