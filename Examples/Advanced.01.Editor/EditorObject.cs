using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Immutable rectangle in a 960 x 540 document; no UI dependencies or reflection.</summary>
internal sealed record EditorObject(int Id, string Name, float X, float Y, float Width, float Height, Color Color, bool Visible = true, float RotationDegrees = 0);
