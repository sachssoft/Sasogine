namespace Sachssoft.Engine.Controls;
/// <summary>
/// One immutable history entry owned by a curve editor.
/// </summary>
internal sealed record CurveEditState(CurvePoint[] Points, CurveInterpolation Interpolation, CurveInterpolation? Override, bool HasLocalOverride);
