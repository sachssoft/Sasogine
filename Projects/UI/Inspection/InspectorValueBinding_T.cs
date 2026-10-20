using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Synchronizes an explicitly described value without discovering members.</summary>
internal sealed class InspectorValueBinding<T> : IInspectorBinding
{
    private readonly Func<T> _read;
    private readonly Action<T> _write, _display;
    private readonly INotifyPropertyChanged? _source;
    private readonly string? _name;
    private readonly IEqualityComparer<T> _comparer;
    private bool _dirty = true, _applying, _hasValue, _connected;
    private T _last = default!;
    internal InspectorValueBinding(Func<T> read, Action<T> write, Action<T> display, INotifyPropertyChanged? source, string? name, IEqualityComparer<T>? comparer = null)
    {
        _read = read;
        _write = write;
        _display = display;
        _source = source;
        _name = name;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public void Connect()
    {
        if (_connected)
            return;
        _connected = true;
        _dirty = true;
        if (_source is not null)
            _source.PropertyChanged += Changed;
    }

    public void Disconnect()
    {
        if (!_connected)
            return;
        if (_source is not null)
            _source.PropertyChanged -= Changed;
        _connected = false;
    }

    private void Changed(object? sender, PropertyChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.PropertyName) || _name is null || args.PropertyName == _name)
            _dirty = true;
    }

    internal void SetValue(T value)
    {
        if (_applying)
            return;
        _write(value);
        _dirty = true;
        Update();
    }

    public void Update()
    {
        if (_applying || (_source is not null && !_dirty))
            return;
        _dirty = false;
        T value = _read();
        if (_hasValue && _comparer.Equals(_last, value))
            return;
        _last = value;
        _hasValue = true;
        _applying = true;
        try
        {
            _display(value);
        }
        finally
        {
            _applying = false;
        }
    }
}
