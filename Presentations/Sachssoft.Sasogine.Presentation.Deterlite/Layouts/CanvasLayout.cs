using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    /// <summary>
    /// CanvasLayout: absolutes Layout-Panel ähnlich WPF Canvas
    /// --------------------------------------------------------
    /// Regeln:
    /// 1. Position der Kinder wird ausschließlich durch Margin.Left / Margin.Top bestimmt.
    /// 2. Margin.Right / Margin.Bottom werden nur berücksichtigt, wenn Width/Height NaN ist.
    /// 3. Width/Height werden immer gegen MinWidth/MaxWidth und MinHeight/MaxHeight geclamped.
    /// 4. Canvas-Padding kann optional die Position der Kinder beeinflussen.
    /// 5. Kinder behalten absolute Position, kein automatischer Fluss oder Alignment.
    /// </summary>
    public class CanvasLayout : LayoutBase
    {
        /// <summary>
        /// Berechnet die gewünschte Größe des Canvas basierend auf den maximalen Extents aller Kinder.
        /// Berücksichtigt: Margin.Left/Top, Width/Height oder DesiredSize, Padding und Canvas-Min/Max.
        /// </summary>
        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children)
            {
                child.Measure(availableSize);
                size.X = float.Max(size.X, child.DesiredSize.X + child.Margin.Right);
                size.Y = float.Max(size.Y, child.DesiredSize.Y + child.Margin.Bottom);
            }
            size.X += Padding.Horizontal;
            size.Y += Padding.Vertical;
            return size;
        }

        /// <summary>
        /// Arrangiert alle Kinder absolut auf dem Canvas.
        /// Regeln:
        /// - Position: Margin.Left / Top
        /// - Größe: Width/Height oder bei NaN über Canvas-Size - Margins
        /// - Clamp auf Min/Max Width/Height
        /// </summary>
        protected override Bounds ArrangeOverride(Bounds finalBounds)
        {
            foreach (var child in Children)
            {
                child.Arrange(new Bounds(
                    finalBounds.X + child.Margin.Left,
                    finalBounds.Y + child.Margin.Top,
                    child.DesiredSize.X,
                    child.DesiredSize.Y
                ));
            }
            return finalBounds;
        }
    }
}