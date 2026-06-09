using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public sealed class FontAsset : AssetBase<FontFace, FontDefinition>
    {
        protected override FontDefinition ResolveDefinition()
        {
            return new FontDefinition();
        }

        protected override FontFace? Build(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
