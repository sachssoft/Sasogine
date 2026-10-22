using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Stores inherited font requests independently of their resolved renderer resource.</summary>
internal readonly record struct ControlFontRequest(string? Name, int? Size, FontWeight? Weight, FontStyle? Style, IControlFont? Source)
{
    internal bool HasDescriptor => Name is not null || Size.HasValue || Weight.HasValue || Style.HasValue;
}
