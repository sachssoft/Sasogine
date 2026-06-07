using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Text
{
    public sealed class FontContainer
    {
        private readonly ResourceSourceBase _loader;

        public FontContainer(ResourceSourceBase loader)
        {
            _loader = loader;
        }

        public ResourceSourceBase Loader => _loader;

        public  FontFace CreateFace(string name, FontWeight weightDefinition = FontWeight.Normal, FontStyle styleDefinition = FontStyle.Normal)
        {
            return new FontFace(_loader, name, weightDefinition, styleDefinition);
        }
    }
}
