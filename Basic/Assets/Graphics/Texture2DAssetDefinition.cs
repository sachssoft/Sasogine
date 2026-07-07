using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Basic.Components.Models;
using Sachssoft.Sasogine.Common.Models;
using Sachssoft.Sasogine.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class Texture2DAssetDefinition : EngineObjectDefinition, ITexture2DAssetDefinition
    {
        [Category(Categories.Appearance)]
        public Texture2DPattern Pattern { get; set; }

        [Category(Categories.Appearance)]
        public Texture2DPatternMode PatternMode { get; set; }

        [Category(Categories.Appearance)]
        public Color DiffuseColor { get; set; }

        [Category(Categories.Appearance)]
        public float Opacity { get; set; }

        [Category(Categories.Rendering)]
        public Texture2DFilterMode FilterMode { get; set; }

        [Category(Categories.Rendering)]
        public Texture2DAddressMode AddressMode { get; set; }

        [Category(Categories.Rendering)]
        public Texture2DFlipMode FlipMode { get; set; }

        [Category(Categories.Rendering)]
        public Texture2DBlendMode BlendMode { get; set; }

        [Category(Categories.Rendering)]
        public bool UseMipmaps { get; set; }

        [Browsable(false)]
        public string? RelativeFullPath { get; set; }
    }
}
