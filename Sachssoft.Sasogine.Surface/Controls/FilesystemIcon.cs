using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public record class FileSystemIcon
    {
        public Func<string, bool> Match { get; }

        public ITextureRegion? Icon { get; }

    }
}
