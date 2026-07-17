using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    /// <summary>
    /// Provides a base implementation for a parallax layer component.
    /// Manages layer configuration, depth ordering, updating, and rendering
    /// of individual parallax layers.
    ///
    /// Derived classes define the specific update and drawing behavior for
    /// different types of parallax content.
    /// </summary>
    public abstract class ParallaxLayerBase<TDefinition> : ResourceComponentBase<TDefinition>
        where TDefinition : ParallaxLayerDefinitionBase
    {
        private int _parallaxIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallaxLayerBase{TDefinition}"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing configuration data for this layer.
        /// </param>
        protected ParallaxLayerBase(TDefinition definition) : base(definition)
        {
        }

        /// <summary>
        /// Updates the state of this parallax layer.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public abstract void Update(SceneUpdateContext context);

        /// <summary>
        /// Draws the content of this parallax layer.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public abstract void Draw(SceneDrawContext context);

        /// <summary>
        /// Calculates the depth factor used for parallax movement based on the layer depth
        /// and camera depth.
        /// </summary>
        /// <param name="cameraDepth">
        /// The current camera depth value.
        /// </param>
        /// <returns>
        /// A factor controlling the relative movement of this layer.
        /// </returns>
        internal protected float GetDepthFactor(float cameraDepth)
        {
            return _parallaxIndex / cameraDepth;
        }

        /// <inheritdoc/>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            _parallaxIndex = Definition.Index;
        }
    }
}
