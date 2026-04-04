using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Presentation.Layouts;
using Sachssoft.Sasogine.Presentation.Styling;
using System;

namespace Sachssoft.Sasogine.Presentation.Rendering
{
    public interface IRenderContext : IWorkspaceObserver
    {
        void Begin(RasterizerState rasterizerState = null!);

        void End();

        // Zeichnen
        void DrawBorder(Bounds rect, Color color, Insets thickness);

        void DrawRectangle(Bounds rect, Color color);

        void DrawText(Bounds rect, string text, Font font, TextLayout layout, Color color);

        Vector2 MeasureText(string text, Font font, float maxWidth = float.PositiveInfinity, TextLayout? layout = null);

        // Clipping
        void PushClip(Bounds rect);

        void PopClip();

        // Transformation / Verschiebung
        void PushTransform(Transform transform);

        void PopTransform();
    }
}
