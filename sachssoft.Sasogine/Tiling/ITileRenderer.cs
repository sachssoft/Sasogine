using Sachssoft.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Tiling;

public interface ITileRenderer
{
    void Draw(Coordinate coordinate);
}
