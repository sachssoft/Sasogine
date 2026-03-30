using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class VerticalWrapPanel : WrapPanel
    {
        public override Orientation Orientation => Orientation.Vertical;

        protected override ElementBase CreateCloneInstance()
        {
            return new VerticalStackPanel();
        }
    }
}
