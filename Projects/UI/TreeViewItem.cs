using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Explicit tree data with retained expansion; no member discovery.
/// </summary>
public class TreeViewItem : Element
{
    /// <summary>
    /// Gets or sets the caption.
    /// </summary>
    private string _Text = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextArgs = new(nameof(Text));
    /// <summary>
    /// Gets or sets Text.
    /// </summary>
    public string Text { get => _Text; set => SetNotify(ref _Text, value, TextArgs); }

    /// <summary>
    /// Gets or sets an application value.
    /// </summary>
    private object? _Value = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    /// <summary>
    /// Gets or sets Value.
    /// </summary>
    public object? Value { get => _Value; set => SetNotify(ref _Value, value, ValueArgs); }

    /// <summary>
    /// Gets child items.
    /// </summary>
    private ElementCollection<TreeViewItem>? _items;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsArgs = new(nameof(Items));
    /// <summary>
    /// Gets items.
    /// </summary>
    public IList<TreeViewItem> Items => _items ??= new(this, ItemsArgs);

    /// <summary>
    /// Gets or sets expansion.
    /// </summary>
    private bool _IsExpanded = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsExpandedArgs = new(nameof(IsExpanded));
    /// <summary>
    /// Gets or sets IsExpanded.
    /// </summary>
    public  bool IsExpanded
    {
        get => _IsExpanded;
        set => SetNotify(ref _IsExpanded, value, IsExpandedArgs);
    }

    /// <summary>
    /// Gets or sets availability inherited by descendants.
    /// </summary>
    private bool _IsEnabled = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsEnabledArgs = new(nameof(IsEnabled));
    /// <summary>
    /// Gets or sets IsEnabled.
    /// </summary>
    public bool IsEnabled { get => _IsEnabled; set => SetNotify(ref _IsEnabled, value, IsEnabledArgs); }

    /// <summary>
    /// Gets or sets an optional atlas icon.
    /// </summary>
    private string? _Icon = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IconArgs = new(nameof(Icon));
    /// <summary>
    /// Gets or sets Icon.
    /// </summary>
    public string? Icon { get => _Icon; set => SetNotify(ref _Icon, value, IconArgs); }
}
