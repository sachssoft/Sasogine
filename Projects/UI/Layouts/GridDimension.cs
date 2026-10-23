using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Observable row or column sizing, independent of any control metadata.
/// </summary>
public abstract class GridDimension : Element
{
    private GridLength _length = GridLength.Proportion();
    private float _minimum, _maximum = float.PositiveInfinity;
    private static readonly PropertyChangedEventArgs LengthArgs = new(nameof(Length)), MinimumArgs = new(nameof(Minimum)), MaximumArgs = new(nameof(Maximum));
    /// <summary>
    /// Gets or sets the row or column length.
    /// </summary>
    public GridLength Length { get => _length; set => SetNotify(ref _length, value, LengthArgs); }
    /// <summary>
    /// Gets or sets the minimum size.
    /// </summary>
    public float Minimum { get => _minimum; set => SetNotify(ref _minimum, value, MinimumArgs); }
    /// <summary>
    /// Gets or sets the maximum size.
    /// </summary>
    public float Maximum { get => _maximum; set => SetNotify(ref _maximum, value, MaximumArgs); }
}
