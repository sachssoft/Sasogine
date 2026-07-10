using Microsoft.Xna.Framework;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Assets.Audio;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Models;
using Sachssoft.Sasogine.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class Texture2DAssetDefinition : AssetDefinitionBase<Texture2DAsset>
    {
        [Category(Categories.Appearance)]
        public Texture2DPattern Pattern { get; set; } = Texture2DPattern.Stretch;

        [Category(Categories.Appearance)]
        public Texture2DPatternMode PatternMode { get; set; } = Texture2DPatternMode.Repeat;

        [Category(Categories.Appearance)]
        public Color DiffuseColor { get; set; } = Color.White;

        [Category(Categories.Appearance)]
        public float Opacity { get; set; } = 1.0f;

        [Category(Categories.Rendering)]
        public Texture2DFilterMode FilterMode { get; set; } = Texture2DFilterMode.Point;

        [Category(Categories.Rendering)]
        public Texture2DAddressMode AddressMode { get; set; } = Texture2DAddressMode.Clamp;

        [Category(Categories.Rendering)]
        public Texture2DFlipMode FlipMode { get; set; } = Texture2DFlipMode.None;

        [Category(Categories.Rendering)]
        public Texture2DBlendMode BlendMode { get; set; } = Texture2DBlendMode.AlphaBlend;

        [Category(Categories.Rendering)]
        public bool UseMipmaps { get; set; } = false;
    }
}
