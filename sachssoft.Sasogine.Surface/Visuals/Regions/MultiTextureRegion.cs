using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a composite image consisting of multiple layered <see cref="IImage"/> instances.
    /// Each layer can have its own offset and color tint.
    /// </summary>
    public class MultiTextureRegion : IImage
    {
        /// <summary>
        /// Holds all layers in this multi-texture region.
        /// Each entry contains the image, its offset, and its color tint.
        /// </summary>
        private readonly List<(IImage image, Point offset, Color color)> _layers = new();

        /// <summary>
        /// Gets the list of layers as tuples containing:
        /// <c>(IImage image, Point offset, Color color)</c>.
        /// </summary>
        public IReadOnlyList<(IImage image, Point offset, Color color)> Layers => _layers;

        /// <summary>
        /// Gets the size of this multi-texture region.
        /// It uses the size of the first layer as reference. If there are no layers, returns <see cref="Point.Zero"/>.
        /// </summary>
        public Point Size => _layers.Count > 0 ? _layers[0].image.Size : Point.Zero;

        /// <summary>
        /// Adds a new layer to this multi-texture region.
        /// </summary>
        /// <param name="image">The <see cref="IImage"/> to add as a layer.</param>
        /// <param name="offset">Optional pixel offset applied when rendering this layer. Defaults to <see cref="Point.Zero"/>.</param>
        /// <param name="color">Optional color tint applied to this layer. Defaults to <see cref="Color.White"/>.</param>
        public void AddLayer(IImage image, Point? offset = null, Color? color = null)
        {
            if (image != null)
            {
                _layers.Add((
                    image,
                    offset ?? Point.Zero,
                    color ?? Color.White
                ));
            }
        }

        /// <summary>
        /// Draws the composite image by rendering all layers in order.
        /// Each layer is drawn with its own offset and color tint, combined with the provided <paramref name="color"/>.
        /// </summary>
        /// <param name="context">The rendering context containing the SpriteBatch and state.</param>
        /// <param name="dest">The destination rectangle where the image should be drawn.</param>
        /// <param name="color">A global color tint applied to all layers. This is multiplied with each layer's color tint.</param>
        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            foreach (var (image, offset, layerColor) in _layers)
            {
                // Calculate destination rectangle for this layer
                var layerDest = new Rectangle(
                    dest.X + offset.X,
                    dest.Y + offset.Y,
                    dest.Width,
                    dest.Height
                );

                // Combine global and layer-specific colors
                var combinedColor = color.ToVector4() * layerColor.ToVector4();

                // Draw the layer with the combined tint
                image.Draw(context, layerDest, new Color(combinedColor));
            }
        }
    }
}
