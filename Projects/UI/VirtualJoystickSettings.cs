using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay.Curves;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Typed polling snapshot; equality requires no property lookup.
/// </summary>
internal readonly record struct VirtualJoystickSettings(JoystickMovementMode MovementMode, JoystickPlacementMode PlacementMode, Vector2 CustomAxis, float Radius, Size2 ThumbSize, float DeadZone, bool ReturnToCenter, bool InvertX, bool InvertY, CurveBase? ResponseCurve);
