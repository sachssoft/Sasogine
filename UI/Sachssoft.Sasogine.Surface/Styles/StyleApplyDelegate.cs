using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public delegate void StyleApplyDelegate<T>(T source, Stylesheet stylesheet, string property, PropertyValue value) where T : ElementBase;
}
