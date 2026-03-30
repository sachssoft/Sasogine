using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        public event EventHandler? MouseMoved;

        public event EventHandler? TouchMoved;
        public event EventHandler? TouchDown;
        public event EventHandler? TouchUp;
        public event EventHandler? TouchDoubleClick;

        public event EventHandler<GenericEventArgs<float>>? MouseWheelChanged;

        public event EventHandler<GenericEventArgs<Keys>>? KeyUp;
        public event EventHandler<GenericEventArgs<Keys>>? KeyDown;
        public event EventHandler<GenericEventArgs<char>>? Char;

        public event EventHandler<CancellableEventArgs<Widget>>? ContextMenuClosing;
        public event EventHandler<GenericEventArgs<Widget>>? ContextMenuClosed;

        public event EventHandler<CancellableEventArgs<Widget>>? WidgetLosingKeyboardFocus;
        public event EventHandler<GenericEventArgs<Widget>>? WidgetGotKeyboardFocus;

        public event EventHandler? CultureChanged;
        public event EventHandler? SceneChanged;

        public event EventHandler? LayoutChanged;
    }
}
