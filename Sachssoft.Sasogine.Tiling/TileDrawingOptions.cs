using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Optionen zur Steuerung des Tile-Zeichnens,
    /// inklusive Größe, Farbe, Transparenz und Transformation.
    /// </summary>
    public class TileDrawingOptions
    {
        #region Rendering

        /// <summary>
        /// Die Größe des Tiles in Pixeln (Breite und Höhe).
        /// </summary>
        public Vector2 TileSize { get; set; } = new Vector2(10f);

        /// <summary>
        /// Die Tiefenlage des Tiles beim Zeichnen (Layer-Depth).
        /// Wertebereich typischerweise 0 (vorne) bis 1 (hinten).
        /// </summary>
        public float LayerDepth { get; set; } = 0f;

        #endregion

        #region Visual

        /// <summary>
        /// Die Farbe, mit der das Tile gezeichnet wird.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Die Transparenz des Tiles.
        /// 1 = vollständig sichtbar, 0 = unsichtbar.
        /// </summary>
        public float Opacity { get; set; } = 1f;

        #endregion

        #region Transformation

        /// <summary>
        /// Die Transformationsmatrix für das Tile
        /// (beinhaltet Translation, Skalierung, Rotation etc.).
        /// </summary>
        public Matrix TransformMatrix { get; set; } = Matrix.Identity;

        /// <summary>
        /// Der Ursprungspunkt für Transformationen,
        /// relativ zur Tile-Größe (0..1, z.B. 0.5 = Mitte).
        /// </summary>
        public Vector2 Origin { get; set; } = new Vector2(0.5f);

        /// <summary>
        /// Pixelbasierte Verschiebung (Offset) für das Tile.
        /// </summary>
        public Vector2 Offset { get; set; } = Vector2.Zero;

        /// <summary>
        /// Rotationswinkel in Bogenmaß (Radiant), der auf das Tile angewendet wird.
        /// </summary>
        public float Rotation { get; set; } = 0f;

        /// <summary>
        /// Skalierungsfaktor (X,Y) für das Tile.
        /// </summary>
        public Vector2 Scale { get; set; } = Vector2.One;
        public TileDrawingOptions Clone()
        {
            return new TileDrawingOptions
            {
                Color = this.Color,
                Opacity = this.Opacity,
                Scale = this.Scale,
                Rotation = this.Rotation,
                Origin = this.Origin,
                Offset = this.Offset,
                LayerDepth = this.LayerDepth,
                TransformMatrix = this.TransformMatrix
                // ggf. alle Felder kopieren
            };
        }

        public void Reset()
        {
            TileSize = new Vector2(10f);
            LayerDepth = 0f;

            Color = Color.White;
            Opacity = 1f;

            TransformMatrix = Matrix.Identity;
            Origin = new Vector2(0.5f);
            Offset = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        #endregion
    }
}
