using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Identifies one rendered size of a resolved, immutable engine face.</summary>
internal readonly record struct ControlFontKey(FontFace Face, int Size);
