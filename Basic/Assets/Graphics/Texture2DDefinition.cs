using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class Texture2DDefinition : AssetDefinitionBase
    {
        public Texture2DAssetType AssetType { get; set; }

        public Vector2 Translation { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Vector2 Pivot { get; set; }

        public Texture2DPattern Pattern { get; set; }

        public Texture2DPatternMode PatternMode { get; set; }

        public Color? DiffuseColor { get; set; }

        public float Opacity { get; set; }

        public int Layer { get; set; }

        public Texture2DFilterMode FilterMode { get; set; }

        public Texture2DAddressMode AddressMode { get; set; }

        public SpriteEffects SpriteEffect { get; set; }

        public Texture2DBlendMode BlendMode { get; set; }

        public bool UseMipmaps { get; set; }
    }
}
