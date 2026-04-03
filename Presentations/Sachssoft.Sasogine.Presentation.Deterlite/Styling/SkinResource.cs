using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public class SkinResource
    {
        public SkinResource() { }

        public string? Id { get; set; }

        public string? Class { get; set; }

        public ResourceFileSource Source { get; set; }

        public PropertyMap Values { get; } = new PropertyMap();


    }
}
