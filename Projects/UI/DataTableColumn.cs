using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Explicit column configuration and row access; no member-name lookup is performed.</summary>
public abstract class DataTableColumn : Element
{
    /// <summary>Gets or sets a stable application ID.</summary>
    private string _Id = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IdArgs = new(nameof(Id));
    /// <summary>Gets or sets Id.</summary>
    public string Id { get => _Id; set => SetNotify(ref _Id, value, IdArgs); }

    /// <summary>Gets or sets the header caption.</summary>
    private string _Header = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderArgs = new(nameof(Header));
    /// <summary>Gets or sets Header.</summary>
    public string Header { get => _Header; set => SetNotify(ref _Header, value, HeaderArgs); }

    /// <summary>Gets or sets logical width.</summary>
    private float _Width = 160;
    private static readonly System.ComponentModel.PropertyChangedEventArgs WidthArgs = new(nameof(Width));
    /// <summary>Gets or sets Width.</summary>
    public float Width { get => _Width; set => SetNotify(ref _Width, value, WidthArgs); }

    /// <summary>Gets or sets minimum width.</summary>
    private float _MinWidth = 36;
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinWidthArgs = new(nameof(MinWidth));
    /// <summary>Gets or sets MinWidth.</summary>
    public float MinWidth { get => _MinWidth; set => SetNotify(ref _MinWidth, value, MinWidthArgs); }

    /// <summary>Gets or sets maximum width.</summary>
    private float _MaxWidth = float.PositiveInfinity;
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaxWidthArgs = new(nameof(MaxWidth));
    /// <summary>Gets or sets MaxWidth.</summary>
    public float MaxWidth { get => _MaxWidth; set => SetNotify(ref _MaxWidth, value, MaxWidthArgs); }

    /// <summary>Gets or sets visibility.</summary>
    private bool _IsVisible = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsVisibleArgs = new(nameof(IsVisible));
    /// <summary>Gets or sets IsVisible.</summary>
    public bool IsVisible { get => _IsVisible; set => SetNotify(ref _IsVisible, value, IsVisibleArgs); }

    /// <summary>Gets or sets user resizing permission.</summary>
    private bool _CanResize = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanResizeArgs = new(nameof(CanResize));
    /// <summary>Gets or sets CanResize.</summary>
    public bool CanResize { get => _CanResize; set => SetNotify(ref _CanResize, value, CanResizeArgs); }

    /// <summary>Gets or sets user sorting permission.</summary>
    private bool _CanSort = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanSortArgs = new(nameof(CanSort));
    /// <summary>Gets or sets CanSort.</summary>
    public bool CanSort { get => _CanSort; set => SetNotify(ref _CanSort, value, CanSortArgs); }

    /// <summary>Gets or sets whether inline editing is forbidden.</summary>
    private bool _IsReadOnly = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsReadOnlyArgs = new(nameof(IsReadOnly));
    /// <summary>Gets or sets IsReadOnly.</summary>
    public bool IsReadOnly { get => _IsReadOnly; set => SetNotify(ref _IsReadOnly, value, IsReadOnlyArgs); }

    /// <summary>Gets or sets cell text alignment.</summary>
    private ControlAlignment _TextAlignment = ControlAlignment.Start;
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextAlignmentArgs = new(nameof(TextAlignment));
    /// <summary>Gets or sets TextAlignment.</summary>
    public ControlAlignment TextAlignment { get => _TextAlignment; set => SetNotify(ref _TextAlignment, value, TextAlignmentArgs); }

    /// <summary>Formats one row through an explicit accessor.</summary>
    public abstract string GetText(object row);
    /// <summary>Compares two rows. The default compares displayed text ordinally.</summary>
    public virtual int Compare(object left, object right) => StringComparer.Ordinal.Compare(GetText(left), GetText(right));
    /// <summary>Gets whether an explicit setter supports this row.</summary>
    public virtual bool CanEdit(object row) => false;
    /// <summary>Writes an edited value through an explicit setter.</summary>
    public virtual void SetText(object row, string text) => throw new InvalidOperationException("Column is read-only.");
}
