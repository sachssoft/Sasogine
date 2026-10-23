
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a button that maintains a persistent checked state
/// and toggles that state when activated.
/// </summary>
/// <remarks>
/// <para>
/// Unlike a standard push button, a toggle button retains its checked
/// state until it is changed by user interaction or programmatically.
/// </para>
/// <para>
/// Activation changes the checked state before raising
/// <see cref="ButtonBase.Clicked"/> and executing the associated command.
/// </para>
/// <para>
/// The underlying state supports nullable Boolean values, allowing
/// derived controls to represent checked, unchecked, and mixed states.
/// </para>
/// </remarks>
public class ToggleButton : ButtonBase
{
    private StyleProperty<bool?> _styleIsChecked = new(false);

    private static readonly PropertyChangedEventArgs IsCheckedChangedArgs = new(nameof(IsChecked));

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleButton"/> class.
    /// </summary>
    public ToggleButton()
    {
    }

    /// <summary>
    /// Gets or sets whether the button is checked.
    /// </summary>
    /// <remarks>
    /// Setting this property updates the checked state and raises
    /// <see cref="CheckedChanged"/> when the state changes.
    /// An internal mixed state is exposed as <see langword="false"/>.
    /// </remarks>
    public bool IsChecked
    {
        get => ConfigIsChecked == true;
        set => SetChecked(value);
    }

    /// <summary>
    /// Occurs when the checked state changes through <see cref="SetChecked"/>.
    /// </summary>
    /// <remarks>
    /// This event is raised after <see cref="OnCheckedChanged"/>
    /// and before the Clicked event during normal activation.
    /// </remarks>
    public event EventHandler? CheckedChanged;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToggleButton";

    /// <summary>
    /// Updates the checked state and notifies listeners when it changes.
    /// </summary>
    /// <param name="value">
    /// The new checked state, or <see langword="null"/> for a mixed state.
    /// </param>
    /// <remarks>
    /// Invokes <see cref="OnCheckedChanged"/> and raises
    /// <see cref="CheckedChanged"/> only when the value changes.
    /// </remarks>
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

    /// <summary>
    /// Called after the checked state has changed.
    /// </summary>
    /// <remarks>
    /// Derived controls can override this method to implement
    /// additional behavior when the checked state changes.
    /// </remarks>
    protected virtual void OnCheckedChanged()
    {
    }

    /// <summary>
    /// Changes the checked state in response to button activation.
    /// </summary>
    /// <remarks>
    /// The default implementation alternates between checked
    /// and unchecked. Derived controls can override this method
    /// to implement custom state transitions.
    /// </remarks>
    protected virtual void ChangeCheckedState()
    {
        IsChecked = !IsChecked;
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (!CanExecute)
            return;

        ChangeCheckedState();
        base.OnClick();
    }

    /// <summary>
    /// Gets the visual state identifier combining the selection
    /// state with the current interaction state.
    /// </summary>
    /// <remarks>
    /// Selection states are Checked, Unchecked, and Mixed.
    /// Interaction states are Normal, Hover, Pressed, and Disabled.
    /// </remarks>
    protected string SelectionState =>
        (ConfigIsChecked is null ? "Mixed" : IsChecked ? "Checked" : "Unchecked") + "." +
        (!IsAvailable() ? "Disabled" : IsHeld ? "Pressed" : IsPointerOver ? "Hover" : "Normal");

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds, SelectionState))
            context.FillRectangle(LocalBounds, IsChecked ? PressedBackground : Background);
    }

    internal bool? ConfigIsChecked
    {
        get => _styleIsChecked.Value;
        set
        {
            if (_styleIsChecked.SetLocal(value))
                NotifyPropertyChanged(IsCheckedChangedArgs);
        }
    }

    /// <summary>
    /// Applies the control's explicit style values without replacing
    /// locally assigned property values.
    /// </summary>
    /// <param name="style">The style containing the values to apply.</param>
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

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        return property switch
        {
            "IsChecked" => _styleIsChecked.HasLocalValue,
            _ => base.IsStyleLocal(property)
        };
    }

    /// <summary>
    /// Removes a local property override and restores its current style value.
    /// </summary>
    /// <param name="property">The name of the property to reset.</param>
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

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();

        if (_styleIsChecked.CommitStyle())
            NotifyPropertyChanged(IsCheckedChangedArgs);
    }
}
