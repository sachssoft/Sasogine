
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a menu item with optional command execution,
/// checkable state, keyboard shortcut, and submenu items.
/// </summary>
/// <remarks>
/// <para>
/// A menu item can execute an <see cref="ICommand"/>, invoke an
/// <see cref="Action"/>, or provide a collection of submenu items.
/// </para>
/// <para>
/// Checkable items toggle their checked state before executing
/// the associated command and callback.
/// </para>
/// <para>
/// Separators and disabled items cannot be activated.
/// Command availability is determined by
/// <see cref="ICommand.CanExecute(object?)"/>.
/// </para>
/// </remarks>
public sealed class MenuItem : Element
{
    private string _text = string.Empty;
    private bool _isEnabled = true;
    private bool _isSeparator;
    private bool _isCheckable;
    private bool _isChecked;

    private string? _icon;
    private string _shortcutText = string.Empty;
    private Keys _shortcutKey = Keys.None;
    private bool _shortcutControl;
    private bool _shortcutAlt;
    private bool _shortcutShift;

    private ICommand? _command;
    private object? _commandParameter;
    private Action? _action;

    private static readonly PropertyChangedEventArgs TextArgs = new(nameof(Text));
    private static readonly PropertyChangedEventArgs IsEnabledArgs = new(nameof(IsEnabled));
    private static readonly PropertyChangedEventArgs IsSeparatorArgs = new(nameof(IsSeparator));
    private static readonly PropertyChangedEventArgs IsCheckableArgs = new(nameof(IsCheckable));
    private static readonly PropertyChangedEventArgs IsCheckedArgs = new(nameof(IsChecked));
    private static readonly PropertyChangedEventArgs IconArgs = new(nameof(Icon));
    private static readonly PropertyChangedEventArgs ShortcutTextArgs = new(nameof(ShortcutText));
    private static readonly PropertyChangedEventArgs ShortcutKeyArgs = new(nameof(ShortcutKey));
    private static readonly PropertyChangedEventArgs ShortcutControlArgs = new(nameof(ShortcutControl));
    private static readonly PropertyChangedEventArgs ShortcutAltArgs = new(nameof(ShortcutAlt));
    private static readonly PropertyChangedEventArgs ShortcutShiftArgs = new(nameof(ShortcutShift));
    private static readonly PropertyChangedEventArgs CommandArgs = new(nameof(Command));
    private static readonly PropertyChangedEventArgs CommandParameterArgs = new(nameof(CommandParameter));
    private static readonly PropertyChangedEventArgs ActionArgs = new(nameof(Action));

    /// <summary>
    /// Gets or sets the text displayed by the menu item.
    /// </summary>
    /// <remarks>
    /// Ampersands can be used as mnemonic markers and are removed
    /// from the internal display caption.
    /// </remarks>
    public string Text
    {
        get => _text;
        set => SetNotify(ref _text, value, TextArgs);
    }

    /// <summary>
    /// Gets or sets whether the menu item is enabled.
    /// </summary>
    /// <remarks>
    /// Disabled items cannot execute commands or callbacks.
    /// The default value is <see langword="true"/>.
    /// </remarks>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetNotify(ref _isEnabled, value, IsEnabledArgs);
    }

    /// <summary>
    /// Gets or sets whether the item represents a menu separator.
    /// </summary>
    /// <remarks>
    /// Separators are non-interactive and cannot execute commands.
    /// </remarks>
    public bool IsSeparator
    {
        get => _isSeparator;
        set => SetNotify(ref _isSeparator, value, IsSeparatorArgs);
    }

    /// <summary>
    /// Gets or sets whether the item supports a checked state.
    /// </summary>
    /// <remarks>
    /// When enabled, activating the item toggles
    /// <see cref="IsChecked"/> before executing its command.
    /// </remarks>
    public bool IsCheckable
    {
        get => _isCheckable;
        set => SetNotify(ref _isCheckable, value, IsCheckableArgs);
    }

    /// <summary>
    /// Gets or sets whether the menu item is checked.
    /// </summary>
    /// <remarks>
    /// This property is automatically toggled during activation
    /// when <see cref="IsCheckable"/> is enabled.
    /// </remarks>
    public bool IsChecked
    {
        get => _isChecked;
        set => SetNotify(ref _isChecked, value, IsCheckedArgs);
    }

    /// <summary>
    /// Gets or sets the optional icon identifier displayed by the menu item.
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set => SetNotify(ref _icon, value, IconArgs);
    }

    /// <summary>
    /// Gets or sets the shortcut description displayed beside the menu item.
    /// </summary>
    /// <remarks>
    /// This property controls the displayed text only.
    /// Actual shortcut matching uses the shortcut key
    /// and modifier properties.
    /// </remarks>
    public string ShortcutText
    {
        get => _shortcutText;
        set => SetNotify(ref _shortcutText, value, ShortcutTextArgs);
    }

    /// <summary>
    /// Gets or sets the keyboard key associated with the menu shortcut.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="Keys.None"/>,
    /// indicating that no shortcut key is assigned.
    /// </remarks>
    public Keys ShortcutKey
    {
        get => _shortcutKey;
        set => SetNotify(ref _shortcutKey, value, ShortcutKeyArgs);
    }

    /// <summary>
    /// Gets or sets whether the Control modifier is required
    /// for the keyboard shortcut.
    /// </summary>
    public bool ShortcutControl
    {
        get => _shortcutControl;
        set => SetNotify(ref _shortcutControl, value, ShortcutControlArgs);
    }

    /// <summary>
    /// Gets or sets whether the Alt modifier is required
    /// for the keyboard shortcut.
    /// </summary>
    public bool ShortcutAlt
    {
        get => _shortcutAlt;
        set => SetNotify(ref _shortcutAlt, value, ShortcutAltArgs);
    }

    /// <summary>
    /// Gets or sets whether the Shift modifier is required
    /// for the keyboard shortcut.
    /// </summary>
    public bool ShortcutShift
    {
        get => _shortcutShift;
        set => SetNotify(ref _shortcutShift, value, ShortcutShiftArgs);
    }

    /// <summary>
    /// Gets or sets the command executed when the menu item is activated.
    /// </summary>
    /// <remarks>
    /// The command must allow execution through CanExecute
    /// before the menu item can be activated.
    /// </remarks>
    public ICommand? Command
    {
        get => _command;
        set => SetNotify(ref _command, value, CommandArgs);
    }

    /// <summary>
    /// Gets or sets the parameter passed to the command.
    /// </summary>
    /// <remarks>
    /// The parameter is supplied to both CanExecute and Execute.
    /// </remarks>
    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetNotify(ref _commandParameter, value, CommandParameterArgs);
    }

    /// <summary>
    /// Gets or sets an optional callback invoked after command execution.
    /// </summary>
    /// <remarks>
    /// The callback is invoked after the associated command,
    /// or directly when no command is assigned.
    /// </remarks>
    public Action? Action
    {
        get => _action;
        set => SetNotify(ref _action, value, ActionArgs);
    }

    /// <summary>
    /// Gets the collection of child menu items displayed in a submenu.
    /// </summary>
    /// <remarks>
    /// Items can be added or removed through this collection.
    /// The collection itself is not replaceable.
    /// </remarks>
    public IList<MenuItem> Items { get; } = new List<MenuItem>();

    /// <summary>
    /// Gets whether the menu item is currently available for activation.
    /// </summary>
    internal bool CanInvoke =>
        IsEnabled && !IsSeparator && (Command?.CanExecute(CommandParameter) ?? true);

    /// <summary>
    /// Gets the menu caption without mnemonic ampersand markers.
    /// </summary>
    internal string Caption => Text.Replace("&", string.Empty, StringComparison.Ordinal);

    /// <summary>
    /// Executes the menu item's checked-state transition,
    /// command, and optional callback.
    /// </summary>
    /// <remarks>
    /// Does nothing when the item is disabled, is a separator,
    /// or its command cannot execute.
    /// </remarks>
    internal void Execute()
    {
        if (!CanInvoke)
            return;

        if (IsCheckable)
            IsChecked = !IsChecked;

        ICommand? command = Command;
        object? parameter = CommandParameter;

        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);

        Action?.Invoke();
    }
}
