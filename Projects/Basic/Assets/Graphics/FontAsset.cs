using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public sealed class FontAsset : AssetBase<FontFace, FontAssetDefinition>
    {
        public FontAsset() : base(new FontAssetDefinition()) { }

        public FontAsset(FontAssetDefinition definition) : base(definition) { }

        protected override FontAssetDefinition ResolveDefinition()
        {
            return new FontAssetDefinition();
        }

        protected override FontFace? Build(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
