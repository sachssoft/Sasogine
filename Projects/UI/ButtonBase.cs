using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Activates once on a matching pointer or keyboard release.</summary>
public class ButtonBase : ContentControl
{
    private ChangeTracker<bool> _commandAvailable;
    /// <summary>Gets or sets a standard ICommand, including Sasogine Command and Command&lt;T&gt;.</summary>
    public System.Windows.Input.ICommand? Command { get => ConfigCommand; set => ConfigCommand = value; }
    /// <summary>Gets or sets the value passed to CanExecute and Execute.</summary>
    public object? CommandParameter { get => ConfigCommandParameter; set => ConfigCommandParameter = value; }
    /// <summary>Tests the current command and parameter immediately.</summary>
    public bool CanExecute => Command?.CanExecute(CommandParameter) ?? true;

    /// <inheritdoc/>
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

    internal override void Prepare()
    {
        base.Prepare();
        _commandAvailable.EnsureChange(CanExecute);
    }

    private bool _pointerArmed;
    private Keys? _keyArmed;
    /// <summary>Initializes a push button.</summary>
    public ButtonBase() : base()
    {
    }

    /// <summary>Gets whether a pointer or keyboard press is currently armed.</summary>
    public virtual bool IsHeld => (_pointerArmed && IsPointerOver) || _keyArmed.HasValue;

    /// <summary>Occurs on a successful release. Tree changes are deferred until the frame boundary.</summary>
    public event EventHandler? Clicked;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Button";

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

    /// <summary>Activates an available button, including its normal toggle or command behavior.</summary>
    public void PerformClick()
    {
        if (IsAvailable() && CanExecute)
            OnClick();
    }

    /// <summary>Raises the Clicked event.</summary>
    protected virtual void OnClick()
    {
        IsPressed = true;
        var command = Command;
        object? parameter = CommandParameter;
        if (command is not null && !command.CanExecute(parameter))
            return;
        command?.Execute(parameter);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Reports activation without executing a primary command, for popup-only buttons.</summary>
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
        Color background = !IsAvailable() ? DisabledBackground : IsHeld ? PressedBackground : IsPointerOver ? HoverBackground : Background;
        var bounds = new Bounds2(0, 0, Bounds.Width, Bounds.Height);
        string state = !IsAvailable() ? "Disabled" : IsHeld ? "Pressed" : IsPointerOver ? "Hover" : "Normal";
        if (!context.DrawSkinPart(this, "Background", bounds, state))
            context.FillRectangle(bounds, background);
    }

    private StyleProperty<System.Windows.Input.ICommand?> _styleCommand = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandChangedArgs = new(nameof(Command));
    internal System.Windows.Input.ICommand? ConfigCommand
    {
        get => _styleCommand.Value;
        set
        {
            if (_styleCommand.SetLocal(value))
                NotifyPropertyChanged(CommandChangedArgs);
        }
    }

    private StyleProperty<object?> _styleCommandParameter = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandParameterChangedArgs = new(nameof(CommandParameter));
    internal object? ConfigCommandParameter
    {
        get => _styleCommandParameter.Value;
        set
        {
            if (_styleCommandParameter.SetLocal(value))
                NotifyPropertyChanged(CommandParameterChangedArgs);
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
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<System.Windows.Input.ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;
                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Command":
                return _styleCommand.HasLocalValue;
            case "CommandParameter":
                return _styleCommandParameter.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
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

    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandEnabledArgs = new(nameof(IsEnabled));
    private bool _isPressed, _isReleased, _notifiedHeld;
    private static readonly System.ComponentModel.PropertyChangedEventArgs PressedArgs = new(nameof(IsPressed)), ReleasedArgs = new(nameof(IsReleased)), HeldArgs = new(nameof(IsHeld));
    /// <summary>Gets whether activation occurred during the current update.</summary>
    public bool IsPressed { get => _isPressed; private set => SetNotify(ref _isPressed, value, PressedArgs); }
    /// <summary>Gets whether a held input was released during the current update.</summary>
    public bool IsReleased { get => _isReleased; private set => SetNotify(ref _isReleased, value, ReleasedArgs); }

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

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
    }
}
