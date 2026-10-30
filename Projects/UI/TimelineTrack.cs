using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Represents one timeline row containing independently timed blocks.</summary>
public sealed class TimelineTrack : Element
{
    /// <summary>Identifies changes to row editing permission.</summary>
    private static readonly PropertyChangedEventArgs IsLockedArgs = new(nameof(IsLocked));

    /// <summary>Forwards structural changes and block property notifications.</summary>
    private static readonly PropertyChangedEventArgs ItemsArgs = new(nameof(Items));

    /// <summary>Identifies changes to application data.</summary>
    private static readonly PropertyChangedEventArgs TagArgs = new(nameof(Tag));

    /// <summary>Identifies changes to the row caption.</summary>
    private static readonly PropertyChangedEventArgs TitleArgs = new(nameof(Title));

    /// <summary>Stores the observed blocks.</summary>
    private readonly ElementCollection<TimelineItem> _items;

    /// <summary>Stores whether all blocks in the row are locked.</summary>
    private bool _isLocked;

    /// <summary>Stores caller-owned row data.</summary>
    private object? _tag;

    /// <summary>Stores the row caption.</summary>
    private string _title = string.Empty;

    /// <summary>Creates a row with an observed block collection.</summary>
    public TimelineTrack()
    {
        _items = new ElementCollection<TimelineItem>(this, ItemsArgs);
    }

    /// <summary>Gets or sets whether interactive editing is disabled for the entire row.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        set => SetNotify(ref _isLocked, value, IsLockedArgs);
    }

    /// <summary>Gets the blocks; overlaps are permitted and later entries draw above earlier entries.</summary>
    public ElementCollection<TimelineItem> Items => _items;

    /// <summary>Gets or sets application data identifying this animation channel.</summary>
    public object? Tag
    {
        get => _tag;
        set => SetNotify(ref _tag, value, TagArgs);
    }

    /// <summary>Gets or sets the non-null row caption.</summary>
    /// <exception cref="ArgumentNullException">The caption is null.</exception>
    public string Title
    {
        get => _title;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetNotify(ref _title, value, TitleArgs);
        }
    }
}
