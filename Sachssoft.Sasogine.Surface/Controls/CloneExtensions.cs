using Microsoft.Xna.Framework.Audio;
using Sachssoft.Sasogine.Surface.Basic;
using System;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public static class CloneExtensions
    {
        public static object? CloneOrSelf(this object? source)
        {
            if (source is Widget widget)
            {
                return widget.Clone();
            }
            else if (source is ElementBase element)
            {
                return element.Clone(); 
            }
            else if (source is ICloneable cloneable)
            {
                return cloneable.Clone();
            }
            else
            {
                return source;
            }
        }
    }
}
