namespace Sachssoft.Engine.Controls;
/// <summary>
/// One sampled world coordinate; invalid values break the rendered polyline.
/// </summary>
internal readonly record struct CurveSample(double X, double Y, bool IsValid);
