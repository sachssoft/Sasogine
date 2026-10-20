using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>One prepared logo line in canvas-local coordinates.</summary>
internal readonly record struct LogoSegment(Vector2 Start, Vector2 End, Color Color, float Thickness);
