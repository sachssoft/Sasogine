using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public interface ITexture2DDefinition : IAssetDefinition
    {
        Texture2DAssetType AssetType { get; set; }

        Vector2 Translation { get; set; }

        float Rotation { get; set; }

        Vector2 Scale { get; set; }

        Vector2 Pivot { get; set; }

        Texture2DPattern Pattern { get; set; }

        Texture2DPatternMode PatternMode { get; set; }

        Color? DiffuseColor { get; set; }

        float Opacity { get; set; }

        int Layer { get; set; }

        Texture2DFilterMode FilterMode { get; set; }

        Texture2DAddressMode AddressMode { get; set; }

        SpriteEffects SpriteEffect { get; set; }

        Texture2DBlendMode BlendMode { get; set; }

        bool UseMipmaps { get; set; }
    }
}
