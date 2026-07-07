using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public interface ITexture2DAssetDefinition : IAssetDefinition
    {
        public Texture2DPattern Pattern { get; set; }

        public Texture2DPatternMode PatternMode { get; set; }

        public Color DiffuseColor { get; set; }

        public float Opacity { get; set; }

        public Texture2DFilterMode FilterMode { get; set; }

        public Texture2DAddressMode AddressMode { get; set; }

        public Texture2DFlipMode FlipMode { get; set; }

        public Texture2DBlendMode BlendMode { get; set; }

        public bool UseMipmaps { get; set; }
    }
}
