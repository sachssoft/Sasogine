using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Surface
{
    public class ViewContext : IDisposable
    {
        public ViewContext(ViewContext self)
            : this(self.View)
        {
        }

        public ViewContext(ViewBase view)
        {
            View = view;
            Runtime = view.Runtime;
        }

        public IMyGameApp CurrentApp => IMyGameApp.Current;

        public GraphicsDevice GraphicsDevice => IMyGameApp.Current.GraphicsDevice;

        public ViewBase View { get; }

        public RuntimeBase Runtime { get; }

        public TimeSpan BenchmarkTime { get; }

        public virtual void Dispose()
        {
        }
    }
}