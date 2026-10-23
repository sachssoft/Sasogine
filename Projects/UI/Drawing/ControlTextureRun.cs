using Microsoft.Xna.Framework.Graphics;
namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Describes a contiguous index range and its eight caller-owned textures.
/// </summary>
internal readonly record struct ControlTextureRun(int StartIndex, int IndexCount,
    Texture2D? Texture0, Texture2D? Texture1, Texture2D? Texture2, Texture2D? Texture3,
    Texture2D? Texture4, Texture2D? Texture5, Texture2D? Texture6, Texture2D? Texture7)
{
    internal Texture2D? GetTexture(int slot) => slot switch
    {
        0 => Texture0, 1 => Texture1, 2 => Texture2, 3 => Texture3,
        4 => Texture4, 5 => Texture5, 6 => Texture6, _ => Texture7
    };
}
