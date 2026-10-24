namespace Sachssoft.Engine.Controls;
/// <summary>Specifies which window dimensions follow the measured content rather than explicit dimensions.</summary>
public enum SizeToContent
{
    /// <summary>Uses the configured width and height.</summary>
    Manual,
    /// <summary>Fits the width to content while retaining the configured height.</summary>
    Width,
    /// <summary>Fits the height to content while retaining the configured width.</summary>
    Height,
    /// <summary>Fits both dimensions to content.</summary>
    WidthAndHeight
}
