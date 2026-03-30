using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.UI.Deterlite;
using System;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public interface IRenderContext : IDisposable
    {
        void Begin(RasterizerState rasterizerState = null!);
        void End();

        // Zeichnen
        void DrawBorder(Bounds rect, Color color, Insets thickness);
        void DrawRectangle(Bounds rect, Color color);
        void DrawText(SpriteFont font, string text, Vector2 position, Color color);

        // Clipping
        void PushClip(Bounds rect);
        void PopClip();

        // Transformation / Verschiebung
        void PushTransform(Transform transform);
        void PopTransform();
    }
}
