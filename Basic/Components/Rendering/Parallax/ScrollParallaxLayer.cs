using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    /// <summary>
    /// Provides a parallax layer that scrolls continuously with a defined speed.
    ///
    /// The scrolling behavior is controlled by the layer definition.
    /// Rendering of the layer content can be implemented by derived components.
    /// </summary>
    public class ScrollParallaxLayer : ParallaxLayerBase<ScrollParallaxLayerDefinition>
    {
        private Vector2 _offset;


        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollParallaxLayer"/> class
        /// using a default definition.
        /// </summary>
        public ScrollParallaxLayer()
            : base(new ScrollParallaxLayerDefinition())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollParallaxLayer"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing scroll and parallax configuration.
        /// </param>
        public ScrollParallaxLayer(ScrollParallaxLayerDefinition definition)
            : base(definition)
        {
        }


        /// <summary>
        /// Creates a default definition for this parallax layer.
        /// </summary>
        /// <returns>
        /// A new <see cref="ScrollParallaxLayerDefinition"/> instance.
        /// </returns>
        protected override ScrollParallaxLayerDefinition ResolveDefinition()
        {
            return new ScrollParallaxLayerDefinition();
        }


        /// <summary>
        /// Gets the current scrolling offset of this layer.
        /// </summary>
        public Vector2 Offset => _offset;


        /// <summary>
        /// Updates the scrolling position of this parallax layer.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
            float deltaTime = (float)context.GameTime.ElapsedGameTime.TotalSeconds;

            _offset += Definition.ScrollSpeed * deltaTime;
        }


        /// <summary>
        /// Draws the scrolling parallax layer.
        ///
        /// The actual rendering implementation depends on the layer content.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public override void Draw(SceneDrawContext context)
        {
            // Rendering will be implemented by concrete layer types.
        }
    }
}