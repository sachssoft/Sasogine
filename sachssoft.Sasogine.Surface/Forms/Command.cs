/* 
 * © 2024 Tobias Sachs
 * Command
 * 11.07.2024 
*/

using System.Windows.Input;
using System;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Surface.Forms;

[Obsolete("Use Relay Command")]
public class Command : GameObject, ICommand
{
    private Action<ViewBase>? _init;
    private Action<ViewBase>? _execute = null;
    private Func<ViewBase, bool>? _can_execute = null;
    private bool _is_checked = false;
    private bool _is_enabled = true;

    public static Command Create(string label, CommandShortcut shortcut, Action<ViewBase> init, Action<ViewBase> execute, Func<ViewBase, bool>? can_execute = null) =>
        new(init, execute, can_execute)
        {
            Label = label,
            Shortcut = shortcut
        };

    public static Command Create(string label, Action<ViewBase> init, Action<ViewBase> execute, Func<ViewBase, bool>? can_execute = null) =>
        new(init, execute, can_execute)
        {
            Label = label
        };

    public static Command Create(string label, Action<ViewBase> execute) =>
        new(null, execute, null)
        {
            Label = label
        };

    public static Command CreateCheckable(string label, bool is_checked, CommandShortcut shortcut, Action<ViewBase> init, Action<ViewBase> execute, Func<ViewBase, bool>? can_execute = null) =>
        new(init, execute, can_execute)
        {
            IsCheckable = true,
            Label = label,
            Shortcut = shortcut,
            IsChecked = is_checked
        };

    public static Command CreateCheckable(string label, bool is_checked, Action<ViewBase> init, Action<ViewBase>? execute = null, Func<ViewBase, bool>? can_execute = null) =>
        new(init, execute, can_execute)
        {
            IsCheckable = true,
            Label = label,
            IsChecked = is_checked
        };

    public static Command CreateCheckable(string label, bool is_checked, Action<ViewBase>? execute = null) =>
        new(null, execute, null)
        {
            IsCheckable = true,
            Label = label,
            IsChecked = is_checked
        };

    private Command(Action<ViewBase>? init, Action<ViewBase>? execute, Func<ViewBase, bool>? can_execute)
    {
        _init = init;
        _execute = execute;
        _can_execute = can_execute;
    }

    public bool IsCheckable { get; private set; }

    public bool IsChecked
    {
        get => _is_checked;
        set => RaiseAndSetIfChanged(ref _is_checked, value);
    }

    public bool IsEnabled
    {
        get => _is_enabled;
        set => RaiseAndSetIfChanged(ref _is_enabled, value);
    }

    public string? Label { get; private set; }

    public CommandShortcut? Shortcut { get; private set; }

    public void Init(ViewBase view)
    {
        _init?.Invoke(view);
    }

    public void Execute(ViewBase view)
    {
        if (IsCheckable == true)
        {
            IsChecked = !IsChecked;
        }

        _execute?.Invoke(view);
    }

    event EventHandler? ICommand.CanExecuteChanged
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    bool ICommand.CanExecute(object? parameter)
    {
        throw new NotImplementedException();
    }

    void ICommand.Execute(object? parameter)
    {
        throw new NotImplementedException();
    }
}
