namespace Sachssoft.Engine.Controls;
/// <summary>A fixed-capacity chronological ring buffer; appending does not allocate.</summary>
public sealed class ChartSeries : Element
{
    private readonly float[] _samples;
    private int _start, _count;
    private Color _color = new(65, 199, 223);
    private string _name = "";
    private static readonly System.ComponentModel.PropertyChangedEventArgs SamplesArgs = new("Samples"), ColorArgs = new(nameof(Color)), NameArgs = new(nameof(Name));
    /// <summary>Creates a buffer retaining the latest capacity samples.</summary>
    public ChartSeries(int capacity = 240)
    {
        if (capacity < 2)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _samples = new float[capacity];
    }

    /// <summary>Gets the retained sample count.</summary>
    public int Count => _count;
    /// <summary>Gets the fixed buffer capacity.</summary>
    public int Capacity => _samples.Length;

    /// <summary>Gets a chronological sample.</summary>
    public float this[int index] => index >= 0 && index < _count ? _samples[(_start + index) % Capacity] : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Gets or sets the series color.</summary>
    public Color Color { get => _color; set => SetNotify(ref _color, value, ColorArgs); }
    /// <summary>Gets or sets the legend name.</summary>
    public string Name { get => _name; set => SetNotify(ref _name, value, NameArgs); }

    /// <summary>Appends a finite sample and discards the oldest value when full.</summary>
    public void Append(float value)
    {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value));
        if (_count == Capacity)
        {
            _samples[_start] = value;
            _start = (_start + 1) % Capacity;
        }
        else
        {
            _samples[(_start + _count) % Capacity] = value;
            _count++;
        }

        NotifyPropertyChanged(SamplesArgs);
    }

    /// <summary>Removes all samples without resizing the buffer.</summary>
    public void Clear()
    {
        _start = _count = 0;
        NotifyPropertyChanged(SamplesArgs);
    }
}
