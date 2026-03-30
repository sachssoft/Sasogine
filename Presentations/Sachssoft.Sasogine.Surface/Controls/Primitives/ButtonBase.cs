using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Globalization;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives;

public abstract class ButtonBase : ContentControl, ICommandSource, IEnableable
{
    private StyleProperty<bool> _isPressed = new StyleProperty<bool>(false, false);
    private StyleProperty<IBrush?> _pressedBackground = new StyleProperty<IBrush?>(null, false);
    private StyleProperty<ICommand?> _command = new StyleProperty<ICommand?>(null, isUserSet: false);
    private StyleProperty<object?> _commandParameter = new StyleProperty<object?>(null, isUserSet: false);

    private bool _isClicked = false;
    private object? _commandOwner = null;

    public event EventHandler? Click;
    public event EventHandler? PressedChanged;
    public event EventHandler<ValueChangingEventArgs<bool>>? PressedChangingByUser;

    #region Style Properties

    public StyleProperty<bool> IsPressed
    {
        get => _isPressed;
        set
        {
            if (SetAndNotify(ref _isPressed, value))
            {
                OnPressedChanged(EventArgs.Empty);
            }
        }
    }

    public StyleProperty<IBrush?> PressedBackground
    {
        get => _pressedBackground;
        set => SetAndNotify(ref _pressedBackground, value);
    }

    public StyleProperty<ICommand?> Command
    {
        get => _command;
        set
        {
            if (SetAndNotify(ref _command, value, out var oldValue))
            {
                if (oldValue.Value != null)
                    oldValue.Value.CanExecuteChanged -= Command_CanExecuteChanged;

                if (_command.Value != null)
                    _command.Value.CanExecuteChanged += Command_CanExecuteChanged;

                UpdateEffectiveEnabled();
            }
        }
    }

    public StyleProperty<object?> CommandParameter
    {
        get => _commandParameter;
        set
        {
            if (SetAndNotify(ref _commandParameter, value))
            {
                UpdateEffectiveEnabled();
            }
        }
    }

    #endregion

    #region Direct Properties

    public object? CommandOwner
    {
        get => _commandOwner;
        set
        {
            if (SetAndNotify(ref _commandOwner, value))
            {
                UpdateEffectiveEnabled();
            }
        }
    }

    #endregion

    #region Command    

    //protected void TryExecuteCommand()
    //{
    //    var command = Command.Value;

    //    if (command != null && )

    //    //if (command != null && command.CanExecute(new object?[] { this, CommandParameter.Value }))
    //    //    command.Execute(new object?[] { this, CommandParameter.Value });
    //}

    protected override bool ComputeEffectiveEnabled()
    {
        bool baseEnabled = base.ComputeEffectiveEnabled();
        bool commandCanExecute = this.CanExecute();
        //Command.Value?.CanExecute(new object?[] { this, CommandParameter.Value }) ?? true;

        return baseEnabled && commandCanExecute;
    }

    private void Command_CanExecuteChanged(object? sender, EventArgs e)
    {
        UpdateEffectiveEnabled(); // Jetzt berücksichtigt es die gesamte Logik
    }

    protected override void OnIsEffectiveEnabledChanged(EventArgs e)
    {
        base.OnIsEffectiveEnabledChanged(e);
        ContentPresenter?.UpdateEffectiveEnabled();
    }

    protected override void OnContentChanged(EventArgs e)
    {
        base.OnContentChanged(e);
        ContentPresenter?.UpdateEffectiveEnabled();
    }

    #endregion

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(PressedBackground):
                    target.PressedBackground = target.PressedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;

                case nameof(IsPressed):
                    target.IsPressed = target.IsPressed.Override(value.ConvertTo<bool>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ButtonBase source)
            return;

        PressedBackground = source.PressedBackground;
        IsPressed = source.IsPressed;
        Command = source.Command;
        CommandParameter = source.CommandParameter;
    }

    #endregion
    protected void SetValueByUser(bool value)
    {
        if (value != IsPressed)
        {
            var e = new ValueChangingEventArgs<bool>(_isPressed.Value, value);
            OnPressedChangedByUser(e);
            if (e.Cancel) return;
        }

        IsPressed = value;
    }

    protected virtual void OnPressedChangedByUser(ValueChangingEventArgs<bool> e)
    {
        PressedChangingByUser?.Invoke(this, e);
    }

    protected virtual void InternalOnTouchDown() { }
    protected virtual void InternalOnTouchUp() { }

    internal virtual void OnClick(EventArgs e)
    {
        Click?.Invoke(this, e);
    }

    internal protected override void OnTouchDown()
    {
        base.OnTouchDown();
        if (!IsEffectiveEnabled) return;

        InternalOnTouchDown();
        _isClicked = true;
    }

    internal protected override void OnTouchUp()
    {
        base.OnTouchUp();
        if (!IsEffectiveEnabled) return;

        InternalOnTouchUp();

        if (_isClicked && IsEffectiveEnabled)
        {
            _isClicked = false;
            OnClick(EventArgs.Empty);
            this.Execute();
            //TryExecuteCommand();
        }
    }

    protected virtual void OnPressedChanged(EventArgs e)
    {
        PressedChanged?.Invoke(this, e);

        if (ContentPresenter is IPressable pressable)
            pressable.IsPressed = IsPressed;
    }

    public override IBrush? GetCurrentBackground()
    {
        var result = base.GetCurrentBackground();

        if (IsEffectiveEnabled)
        {
            if (IsPressed && PressedBackground.Value != null)
            {
                result = PressedBackground.Value;
            }
            else if (UseOverBackground && HoveredBackground.Value != null)
            {
                result = HoveredBackground.Value;
            }
        }
        else if (DisabledBackground.Value != null)
        {
            result = DisabledBackground.Value;
        }

        return result;
    }

    #region ICommand

    ICommand? ICommandSource.Command
    {
        get => Command.Value;
        set => Command = new StyleProperty<ICommand?>(value);
    }

    object? ICommandSource.CommandParameter
    {
        get => CommandParameter.Value;
        set => CommandParameter = new StyleProperty<object?>(value);
    }

    object? ICommandSource.CommandOwner
    {
        get => CommandOwner;
        set => CommandOwner = (SceneBase?)value;
    }

    #endregion
}