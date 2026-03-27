using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Widget
    {
        public void Render(RenderContext context, GameTime time)
        {
            if (!IsVisible)
                return;

            // --------------------------
            // Tooltip automatisch anzeigen
            // --------------------------
            if (_lastMouseMovement != null &&
                (Desktop.Tooltip == null || Desktop.Tooltip.Tag != this) &&
                (DateTime.Now - _lastMouseMovement.Value).TotalMilliseconds > UIEnvironment.TooltipDelayInMs)
            {
                var pos = Desktop.MousePosition;
                pos.X += UIEnvironment.TooltipOffset.X;
                pos.Y += UIEnvironment.TooltipOffset.Y;
                Desktop.ShowTooltip(this, pos);
                _lastMouseMovement = null;
            }

            // --------------------------
            // Layout sicherstellen
            // --------------------------
            UpdateArrange();

            // --------------------------
            // Transform & Scissor
            // --------------------------
            var oldTransform = context.Transform;
            context.Transform = Transform;

            Rectangle? oldScissor = null;
            if (ClipToBounds && context.Transform.Rotation == 0)
            {
                oldScissor = context.Scissor;
                var absoluteBounds = context.Transform.Apply(Bounds);
                var newScissor = Rectangle.Intersect(context.Scissor, absoluteBounds);
                if (newScissor.Width == 0 || newScissor.Height == 0)
                {
                    context.Transform = oldTransform;
                    return;
                }
                context.Scissor = newScissor;
            }

            // --------------------------
            // Opacity
            // --------------------------
            var oldOpacity = context.Opacity;
            context.AddOpacity(Opacity);

            // --------------------------
            // Background
            // --------------------------
            var background = GetCurrentBackground();
            background?.Draw(context, BackgroundBounds);

            // --------------------------
            // Border
            // --------------------------
            var border = GetCurrentBorder();
            if (border != null)
            {
                var b = BorderBounds;
                if (BorderThickness.Value.Left > 0)
                    border.Draw(context, new Rectangle(b.X, b.Y, BorderThickness.Value.Left, b.Height));

                if (BorderThickness.Value.Top > 0)
                    border.Draw(context, new Rectangle(b.X, b.Y, b.Width, BorderThickness.Value.Top));

                if (BorderThickness.Value.Right > 0)
                    border.Draw(context, new Rectangle(b.Right - BorderThickness.Value.Right, b.Y, BorderThickness.Value.Right, b.Height));

                if (BorderThickness.Value.Bottom > 0)
                    border.Draw(context, new Rectangle(b.X, b.Bottom - BorderThickness.Value.Bottom, b.Width, BorderThickness.Value.Bottom));
            }

            // --------------------------
            // Internal Rendering (Kinder)
            // --------------------------
            InternalRender(context, time); // rendert nur Children

            // --------------------------
            // Debug Rendering
            // --------------------------
            if (UIEnvironment.DrawWidgetsFrames)
                context.DrawRectangle(Bounds, Color.LightGreen);

            if (UIEnvironment.DrawKeyboardFocusedWidgetFrame && IsKeyboardFocused)
                context.DrawRectangle(Bounds, Color.Red);

            if (UIEnvironment.DrawMouseHoveredWidgetFrame && IsMouseInside)
                context.DrawRectangle(Bounds, Color.Yellow);

            // --------------------------
            // Restore
            // --------------------------
            if (oldScissor != null)
                context.Scissor = oldScissor.Value;

            context.Transform = oldTransform;
            context.Opacity = oldOpacity;
        }

        public virtual void InternalRender(RenderContext context, GameTime time)
        {
            foreach (var child in LayoutChildren)
            {
                if (child.IsVisible)
                    child.Render(context, time);
            }
        }

        public virtual IBrush? GetCurrentBackground()
        {
            var result = Background;

            if (!IsEnabled && DisabledBackground.Value != null)
            {
                result = DisabledBackground;
            }
            else if (IsEnabled && IsKeyboardFocused && FocusedBackground.Value != null)
            {
                result = FocusedBackground;
            }
            else if (UseOverBackground && HoveredBackground.Value != null)
            {
                result = HoveredBackground;
            }

            return result.Value;
        }

        public virtual IBrush? GetCurrentBorder()
        {
            var result = Border;

            if (!IsEnabled && DisabledBorder.Value != null)
            {
                result = DisabledBorder;
            }
            else if (IsEnabled && IsKeyboardFocused && FocusedBorder.Value != null)
            {
                result = FocusedBorder;
            }
            else if (IsMouseInside && HoveredBorder.Value != null)
            {
                result = HoveredBorder;
            }

            return result.Value;
        }
    }
}
