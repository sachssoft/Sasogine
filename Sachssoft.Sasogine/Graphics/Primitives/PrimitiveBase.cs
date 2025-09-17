using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    public abstract class PrimitiveBase
    {
        /// <summary>
        /// Füllfarbe des Primitives
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Sichtbarkeit steuern
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Optionaler Ausschnitt der Textur (für Texture Atlas).
        /// Falls null, wird die gesamte Textur verwendet.
        /// </summary>
        public Rectangle? SourceRect { get; set; }

        /// <summary>
        /// Anzahl der Vertices
        /// </summary>
        public abstract int VertexCount { get; }

        /// <summary>
        /// Anzahl der Indices
        /// </summary>
        public abstract int IndexCount { get; }

        /// <summary>
        /// Füllt Vertex- und Indexdaten in den Buffer.
        /// </summary>
        public abstract void Fill(
            VertexPositionColorNormalTexture[] vertices,
            int vertexOffset,
            short[] indices,
            int indexOffset,
            short baseVertex,
            Texture2D? texture = null
        );

        /// <summary>
        /// Wird pro Frame aufgerufen (z. B. für Animation, Transformation etc.)
        /// </summary>
        public virtual void Update(GameContext context) { }

        /// <summary>
        /// Berechnet UV-Koordinaten aus SourceRect und Texture.
        /// </summary>
        protected (float u0, float v0, float u1, float v1) GetUV(Texture2D? texture)
        {
            if (texture == null || !SourceRect.HasValue)
            {
                return (0f, 0f, 1f, 1f); // Ganze Textur
            }

            var src = SourceRect.Value;
            float u0 = (float)src.X / texture.Width;
            float v0 = (float)src.Y / texture.Height;
            float u1 = (float)(src.X + src.Width) / texture.Width;
            float v1 = (float)(src.Y + src.Height) / texture.Height;

            return (u0, v0, u1, v1);
        }
    }
}
