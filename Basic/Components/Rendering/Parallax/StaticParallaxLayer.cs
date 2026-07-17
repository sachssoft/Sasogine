using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    /// <summary>
    /// Provides a static parallax layer that remains fixed during camera movement.
    ///
    /// This layer does not apply scrolling or camera-based offset calculations.
    /// It can be used for static background elements that are rendered as part
    /// of a parallax composition.
    /// </summary>
    public class StaticParallax : ParallaxLayerBase<StaticParallaxLayerDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticParallax"/> class
        /// using a default definition.
        /// </summary>
        public StaticParallax()
            : base(new StaticParallaxLayerDefinition())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StaticParallax"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing configuration data for this layer.
        /// </param>
        public StaticParallax(StaticParallaxLayerDefinition definition)
            : base(definition)
        {
        }


        /// <summary>
        /// Creates a default definition for this parallax layer.
        /// </summary>
        /// <returns>
        /// A new <see cref="StaticParallaxLayerDefinition"/> instance.
        /// </returns>
        protected override StaticParallaxLayerDefinition ResolveDefinition()
        {
            return new StaticParallaxLayerDefinition();
        }


        /// <summary>
        /// Updates the static parallax layer.
        ///
        /// No update is required because this layer does not move.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
        }


        /// <summary>
        /// Draws the static parallax layer content.
        ///
        /// The actual rendering implementation depends on the layer content.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public override void Draw(SceneDrawContext context)
        {
        }
    }
}