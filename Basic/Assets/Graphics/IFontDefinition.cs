using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public interface IFontDefinition : IAssetDefinition
    {

        public FontWeight WeightDefinition { get; set; }

        public FontStyle StyleDefinition { get; set; }

    }
}
