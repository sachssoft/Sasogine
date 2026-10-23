
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Provides the base implementation for controls that activate through
/// pointer input, keyboard input, or programmatic invocation.
/// </summary>
/// <remarks>
/// <para>
/// Activation occurs when the primary pointer button or an activation key
/// is released after a matching press.
/// </para>
/// <para>
/// The <see cref="Clicked"/> event is raised before executing
/// <see cref="Command"/>, following the activation order used by
/// WPF and Avalonia.
/// </para>
/// <para>
/// Commands implement <see cref="ICommand"/> and support
/// <see cref="ICommand.CanExecute(object?)"/> for controlling availability.
/// </para>
/// </remarks>
public class ButtonBase : ContentControl
{
    private ChangeTracker<bool> _commandAvailable;

    private bool _pointerArmed;
    private Keys? _keyArmed;

    private bool _isPressed;
    private bool _isReleased;
    private bool _notifiedHeld;

    private StyleProperty<ICommand?> _styleCommand = new(null);
    private StyleProperty<object?> _styleCommandParameter = new(null);

    private static readonly PropertyChangedEventArgs CommandChangedArgs = new(nameof(Command));
    private static readonly PropertyChangedEventArgs CommandParameterChangedArgs = new(nameof(CommandParameter));
    private static readonly PropertyChangedEventArgs CommandEnabledArgs = new(nameof(IsEnabled));
    private static readonly PropertyChangedEventArgs PressedArgs = new(nameof(IsPressed));
    private static readonly PropertyChangedEventArgs ReleasedArgs = new(nameof(IsReleased));
    private static readonly PropertyChangedEventArgs HeldArgs = new(nameof(IsHeld));

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonBase"/> class.
    /// </summary>
    public ButtonBase()
    {
    }

    /// <summary>
    /// Gets or sets the command executed after the <see cref="Clicked"/> event.
    /// </summary>
    /// <remarks>
    /// Supports standard <see cref="ICommand"/> implementations,
    /// including Sasogine Command and Command&lt;T&gt;.
    /// A command that cannot execute makes the button unavailable.
    /// </remarks>
    public ICommand? Command
    {
        get => ConfigCommand;
        set => ConfigCommand = value;
    }

    /// <summary>
    /// Gets or sets the parameter passed to the command's
    /// CanExecute and Execute methods.
    /// </summary>
    public object? CommandParameter
    {
        get => ConfigCommandParameter;
        set => ConfigCommandParameter = value;
    }

    /// <summary>
    /// Gets whether the current command can execute with the current parameter.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> when no command is assigned.
    /// This property evaluates the command immediately.
    /// </remarks>
    public bool CanExecute => Command?.CanExecute(CommandParameter) ?? true;

    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    /// <remarks>
    /// The effective enabled state combines the inherited enabled state
    /// with the availability of the current command.
    /// </remarks>
    public override bool IsEnabled
    {
        get
        {
            if (!_commandAvailable.WasEnsured)
                _commandAvailable.Set(CanExecute);

            return base.IsEnabled && _commandAvailable.NewValue;
        }
        set => base.IsEnabled = value;
    }

    /// <summary>
    /// Gets whether a pointer or keyboard activation is currently held.
    /// </summary>
    /// <remarks>
    /// Pointer activation remains held only while the pointer is over
    /// the control. Keyboard activation remains held until the matching
    /// key is released or the interaction is cancelled.
    /// </remarks>
    public virtual bool IsHeld => (_pointerArmed && IsPointerOver) || _keyArmed.HasValue;

    /// <summary>
    /// Gets whether the button was activated during the current update frame.
    /// </summary>
    public bool IsPressed
    {
        get => _isPressed;
        private set => SetNotify(ref _isPressed, value, PressedArgs);
    }

    /// <summary>
    /// Gets whether a held activation input was released during
    /// the current update frame.
    /// </summary>
    public bool IsReleased
    {
        get => _isReleased;
        private set => SetNotify(ref _isReleased, value, ReleasedArgs);
    }

    /// <summary>
    /// Occurs when the button is successfully activated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event is raised before the associated command executes.
    /// </para>
    /// <para>
    /// Changes to the control tree are deferred until the frame boundary.
    /// </para>
    /// </remarks>
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Button";

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        _commandAvailable.EnsureChange(CanExecute);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        ControlInputEvent input = args.Input;

        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            context.CapturePointer(this);
            _pointerArmed = true;
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left && _pointerArmed)
        {
            bool click = IsPointerOver;

            _pointerArmed = false;
            IsReleased = true;
            context.ReleasePointer(this);
            args.Handled = true;

            if (click)
                OnClick();
        }
        else if (input.Type == ControlInputEventType.KeyDown && IsFocused && input.Key is Keys.Enter or Keys.Space)
        {
            if (!input.IsRepeat && !_keyArmed.HasValue)
                _keyArmed = input.Key;

            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.KeyUp && IsFocused && _keyArmed == input.Key)
        {
            _keyArmed = null;
            IsReleased = true;
            args.Handled = true;

            OnClick();
        }
    }

    /// <summary>
    /// Programmatically activates the button when it is available.
    /// </summary>
    /// <remarks>
    /// Performs the same activation sequence as user input:
    /// raises <see cref="Clicked"/> first, then executes the command.
    /// Does nothing when the button is unavailable or the command
    /// cannot execute.
    /// </remarks>
    public void PerformClick()
    {
        if (IsAvailable() && CanExecute)
            OnClick();
    }

    /// <summary>
    /// Processes a successful button activation.
    /// </summary>
    /// <remarks>
    /// Raises <see cref="Clicked"/> before executing the command.
    /// The command and parameter are evaluated again after the event,
    /// allowing event handlers to change either value.
    /// </remarks>
    protected virtual void OnClick()
    {
        if (!CanExecute)
            return;

        IsPressed = true;
        Clicked?.Invoke(this, EventArgs.Empty);

        ICommand? command = Command;
        object? parameter = CommandParameter;

        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);
    }

    /// <summary>
    /// Raises the <see cref="Clicked"/> event without executing
    /// the primary command.
    /// </summary>
    /// <remarks>
    /// Intended for derived controls that provide alternative activation
    /// behavior, such as opening a popup.
    /// </remarks>
    protected void RaiseClicked()
    {
        IsPressed = true;
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _pointerArmed = false;
        _keyArmed = null;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        Color background = !IsAvailable() ? DisabledBackground
            : IsHeld ? PressedBackground
            : IsPointerOver ? HoverBackground
            : Background;

        string state = !IsAvailable() ? "Disabled"
            : IsHeld ? "Pressed"
            : IsPointerOver ? "Hover"
            : "Normal";

        var bounds = new Bounds2(0, 0, Bounds.Width, Bounds.Height);

        if (!context.DrawSkinPart(this, "Background", bounds, state))
            context.FillRectangle(bounds, background);
    }

    /// <inheritdoc/>
    internal override void BeginFrame()
    {
        base.BeginFrame();
        IsPressed = IsReleased = false;
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);

        if (_commandAvailable.EnsureChange(CanExecute))
            NotifyPropertyChanged(CommandEnabledArgs);

        if (_notifiedHeld != IsHeld)
        {
            _notifiedHeld = IsHeld;
            NotifyPropertyChanged(HeldArgs);
        }
    }

    internal ICommand? ConfigCommand
    {
        get => _styleCommand.Value;
        set
        {
            if (_styleCommand.SetLocal(value))
                NotifyPropertyChanged(CommandChangedArgs);
        }
    }

    internal object? ConfigCommandParameter
    {
        get => _styleCommandParameter.Value;
        set
        {
            if (_styleCommandParameter.SetLocal(value))
                NotifyPropertyChanged(CommandParameterChangedArgs);
        }
    }

    /// <summary>
    /// Applies explicit style values without replacing locally assigned properties.
    /// </summary>
    /// <param name="style">The style containing the values to apply.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);

        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;

                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        return property switch
        {
            "Command" => _styleCommand.HasLocalValue,
            "CommandParameter" => _styleCommandParameter.HasLocalValue,
            _ => base.IsStyleLocal(property)
        };
    }

    /// <summary>
    /// Removes a local property override and restores its effective style value.
    /// </summary>
    /// <param name="property">The name of the property to reset.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Command":
                if (_styleCommand.ClearLocal())
                    NotifyPropertyChanged(CommandChangedArgs);
                return;

            case "CommandParameter":
                if (_styleCommandParameter.ClearLocal())
                    NotifyPropertyChanged(CommandParameterChangedArgs);
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

        if (_styleCommand.BeginStyle(null))
            NotifyPropertyChanged(CommandChangedArgs);

        if (_styleCommandParameter.BeginStyle(null))
            NotifyPropertyChanged(CommandParameterChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();

        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);

        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
    }
}
