using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        public void RenderVisual(GameContext context)
        {
            var oldDeviceScissor = _renderContext.DeviceScissor;

            _renderContext.Begin();

            // Disable transform during setting the scissor rectangle for the Desktop
            _renderContext.Transform = Transform;

            var bounds = _renderContext.Transform.Apply(LayoutBounds);
            _renderContext.Scissor = bounds;
            _renderContext.Opacity = Opacity;

            Background.Value?.Draw(_renderContext, LayoutBounds, Color.White);

            foreach (var widget in LayoutChildren)
            {
                if (widget.IsVisible)
                {
                    if (UIEnvironment.EnableModalDarkening && widget is IModalContent modalContent)
                    {
                        modalContent.ModalBackground?.Draw(_renderContext, bounds, Color.White);
                        //_renderContext.FillRectangle(bounds, UIEnvironment.DarkeningColor);
                    }

                    widget.Render(_renderContext, context.GameTime);
                }
            }

            _renderContext.End();

            _renderContext.DeviceScissor = oldDeviceScissor;
        }

        public void Render(PresentationContext context)
        {
            // Layout run
            UpdateLayout();

            // First input run: set Desktop/Widgets input states and schedule input events
            UpdateInput(context);

            _inputContext.Reset();

            var childrenCopy = LayoutChildren;
            for (var i = childrenCopy.Count - 1; i >= 0; --i)
            {
                var widget = childrenCopy[i];

                widget.OnHostMouseMoved(EventArgs.Empty);

                widget.ProcessInput(_inputContext);
            }

            // Only one widget at a time can receive mouse wheel event
            // So scheduling it here
            if (_inputContext.MouseWheelWidget != null)
            {
                InputEventsManager.Queue(_inputContext.MouseWheelWidget, InputEventType.MouseWheel);
            }

            // Second input run: process input events
            InputEventsManager.ProcessEvents();

            // Do another layout run, since an input event could cause the layout change
            UpdateLayout();

            // Render run
            RenderVisual(context);
        }
    }
}
