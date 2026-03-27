using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using System.Reflection.Metadata.Ecma335;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class HorizontalWrapPanel : WrapPanel
    {
        public override Orientation Orientation => Orientation.Horizontal;

        protected override ElementBase CreateCloneInstance()
        {
            return new HorizontalWrapPanel();
        }
    }
}
