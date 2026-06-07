using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public sealed class FontAsset : AssetBase<FontFace, IFontDefinition>
    {
        protected override FontFace? Build(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
