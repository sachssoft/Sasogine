using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>Explicit menu data and command, including submenus and optional shortcuts.</summary>
public sealed class MenuItem : Element
{
    /// <summary>Provides Text.</summary>
    private string _Text = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextArgs = new(nameof(Text));
    /// <summary>Gets or sets Text.</summary>
    public string Text { get => _Text; set => SetNotify(ref _Text, value, TextArgs); }

    /// <summary>Provides IsEnabled.</summary>
    private bool _IsEnabled = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsEnabledArgs = new(nameof(IsEnabled));
    /// <summary>Gets or sets IsEnabled.</summary>
    public bool IsEnabled { get => _IsEnabled; set => SetNotify(ref _IsEnabled, value, IsEnabledArgs); }

    /// <summary>Provides IsSeparator.</summary>
    private bool _IsSeparator = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsSeparatorArgs = new(nameof(IsSeparator));
    /// <summary>Gets or sets IsSeparator.</summary>
    public bool IsSeparator { get => _IsSeparator; set => SetNotify(ref _IsSeparator, value, IsSeparatorArgs); }

    /// <summary>Provides IsCheckable.</summary>
    private bool _IsCheckable = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsCheckableArgs = new(nameof(IsCheckable));
    /// <summary>Gets or sets IsCheckable.</summary>
    public bool IsCheckable { get => _IsCheckable; set => SetNotify(ref _IsCheckable, value, IsCheckableArgs); }

    /// <summary>Provides IsChecked.</summary>
    private bool _IsChecked = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsCheckedArgs = new(nameof(IsChecked));
    /// <summary>Gets or sets IsChecked.</summary>
    public bool IsChecked { get => _IsChecked; set => SetNotify(ref _IsChecked, value, IsCheckedArgs); }

    /// <summary>Provides Icon.</summary>
    private string? _Icon = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IconArgs = new(nameof(Icon));
    /// <summary>Gets or sets Icon.</summary>
    public string? Icon { get => _Icon; set => SetNotify(ref _Icon, value, IconArgs); }

    /// <summary>Provides ShortcutText.</summary>
    private string _ShortcutText = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShortcutTextArgs = new(nameof(ShortcutText));
    /// <summary>Gets or sets ShortcutText.</summary>
    public string ShortcutText { get => _ShortcutText; set => SetNotify(ref _ShortcutText, value, ShortcutTextArgs); }

    /// <summary>Provides ShortcutKey.</summary>
    private Keys _ShortcutKey = Keys.None;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShortcutKeyArgs = new(nameof(ShortcutKey));
    /// <summary>Gets or sets ShortcutKey.</summary>
    public Keys ShortcutKey { get => _ShortcutKey; set => SetNotify(ref _ShortcutKey, value, ShortcutKeyArgs); }

    /// <summary>Provides ShortcutControl.</summary>
    private bool _ShortcutControl = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShortcutControlArgs = new(nameof(ShortcutControl));
    /// <summary>Gets or sets ShortcutControl.</summary>
    public bool ShortcutControl { get => _ShortcutControl; set => SetNotify(ref _ShortcutControl, value, ShortcutControlArgs); }

    /// <summary>Provides ShortcutAlt.</summary>
    private bool _ShortcutAlt = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShortcutAltArgs = new(nameof(ShortcutAlt));
    /// <summary>Gets or sets ShortcutAlt.</summary>
    public bool ShortcutAlt { get => _ShortcutAlt; set => SetNotify(ref _ShortcutAlt, value, ShortcutAltArgs); }

    /// <summary>Provides ShortcutShift.</summary>
    private bool _ShortcutShift = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShortcutShiftArgs = new(nameof(ShortcutShift));
    /// <summary>Gets or sets ShortcutShift.</summary>
    public bool ShortcutShift { get => _ShortcutShift; set => SetNotify(ref _ShortcutShift, value, ShortcutShiftArgs); }

    /// <summary>Provides Command.</summary>
    private System.Windows.Input.ICommand? _Command = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandArgs = new(nameof(Command));
    /// <summary>Gets or sets Command.</summary>
    public System.Windows.Input.ICommand? Command { get => _Command; set => SetNotify(ref _Command, value, CommandArgs); }

    /// <summary>Gets or sets the parameter used for command availability and execution.</summary>
    private object? _CommandParameter = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandParameterArgs = new(nameof(CommandParameter));
    /// <summary>Gets or sets CommandParameter.</summary>
    public object? CommandParameter { get => _CommandParameter; set => SetNotify(ref _CommandParameter, value, CommandParameterArgs); }

    /// <summary>Gets or sets an optional lightweight callback, executed after the command.</summary>
    private Action? _Action = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ActionArgs = new(nameof(Action));
    /// <summary>Gets or sets Action.</summary>
    public Action? Action { get => _Action; set => SetNotify(ref _Action, value, ActionArgs); }
    internal bool CanInvoke => IsEnabled && !IsSeparator && (Command?.CanExecute(CommandParameter) ?? true);
    /// <summary>Gets items.</summary>
    public IList<MenuItem> Items { get; } = new List<MenuItem>();
    internal string Caption => Text.Replace("&", string.Empty, StringComparison.Ordinal);

    internal void Execute()
    {
        if (!CanInvoke)
            return;
        if (IsCheckable)
            IsChecked = !IsChecked;
        Command?.Execute(CommandParameter);
        Action?.Invoke();
    }
}
