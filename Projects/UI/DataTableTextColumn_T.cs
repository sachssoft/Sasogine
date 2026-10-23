using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// AOT-safe typed text column with optional editing and typed sorting.
/// </summary>
/// <typeparam name = "T">Reference type of the application row.</typeparam>
public sealed class DataTableTextColumn<T> : DataTableColumn where T : class
{
    private readonly Func<T, string> _get;
    private readonly Action<T, string>? _set;
    private readonly Comparison<T>? _compare;
    /// <summary>
    /// Creates an explicitly bound column.
    /// </summary>
    public DataTableTextColumn(string header, Func<T, string> getText, Action<T, string>? setText = null, Comparison<T>? comparison = null)
    {
        ArgumentNullException.ThrowIfNull(getText);
        Header = header;
        _get = getText;
        _set = setText;
        _compare = comparison;
    }

    /// <inheritdoc/>
    public override string GetText(object row) => row is T typed ? _get(typed) ?? string.Empty : throw new ArgumentException("Row is incompatible with this typed column.", nameof(row));
    /// <inheritdoc/>
    public override int Compare(object left, object right) => _compare is null ? base.Compare(left, right) : left is T a && right is T b ? _compare(a, b) : throw new ArgumentException("Rows are incompatible with this typed column.");
    /// <inheritdoc/>
    public override bool CanEdit(object row) => !IsReadOnly && _set is not null && row is T;
    /// <inheritdoc/>
    public override void SetText(object row, string text)
    {
        if (!CanEdit(row) || row is not T typed)
            throw new InvalidOperationException("Column is read-only or the row type is incompatible.");
        _set!(typed, text);
    }
}
