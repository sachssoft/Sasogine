namespace Sachssoft.Engine.Controls;
/// <summary>
/// Explicit Sasogine color representations offered by the editor.
/// </summary>
public enum ColorPickerModel
{
    /// <summary>
    /// Byte RGBA channels.
    /// </summary>
    Rgba,
    /// <summary>
    /// RgbColor with normalized channels.
    /// </summary>
    Rgb,
    /// <summary>
    /// NormalizedColor with normalized channels.
    /// </summary>
    NormalizedRgb,
    /// <summary>
    /// HslColor: degrees and percentages.
    /// </summary>
    Hsl,
    /// <summary>
    /// HsvColor: degrees and percentages.
    /// </summary>
    Hsv,
    /// <summary>
    /// LinearRgbColor with linear channels.
    /// </summary>
    LinearRgb,
    /// <summary>
    /// OklabColor with L, a, b and alpha.
    /// </summary>
    Oklab
}
