using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    /// <summary>
    /// Provides a parallax layer that follows camera movement with a configurable
    /// depth factor.
    ///
    /// The layer calculates its offset based on camera movement and the configured
    /// parallax depth. Rendering content can be added by overriding the draw logic
    /// or extending this component.
    /// </summary>
    public class FollowParallaxLayer : ParallaxLayerBase<FollowParallaxLayerDefinition>
    {
        private Vector2 _offset;
        private Vector2 _lastCameraPosition;


        /// <summary>
        /// Initializes a new instance of the <see cref="FollowParallaxLayer"/> class
        /// using a default definition.
        /// </summary>
        public FollowParallaxLayer()
            : base(new FollowParallaxLayerDefinition())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FollowParallaxLayer"/> class.
        /// </summary>
        /// <param name="definition">
        /// The definition containing parallax layer configuration.
        /// </param>
        public FollowParallaxLayer(FollowParallaxLayerDefinition definition)
            : base(definition)
        {
        }


        /// <summary>
        /// Creates a default definition for this parallax layer.
        /// </summary>
        /// <returns>
        /// A new <see cref="FollowParallaxLayerDefinition"/> instance.
        /// </returns>
        protected override FollowParallaxLayerDefinition ResolveDefinition()
        {
            return new FollowParallaxLayerDefinition();
        }


        /// <summary>
        /// Gets the calculated parallax offset.
        /// </summary>
        public Vector2 Offset => _offset;


        /// <summary>
        /// Updates the parallax layer based on camera movement.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public override void Update(SceneUpdateContext context)
        {
            if (context.Cameras.Length == 0 || context.Cameras[0] is not ICamera2D camera2D)
                return;


            Vector2 cameraPosition = camera2D.Position;


            Vector2 movement = cameraPosition - _lastCameraPosition;


            _offset += movement * GetDepthFactor(Definition.Depth);


            _lastCameraPosition = cameraPosition;
        }


        /// <summary>
        /// Draws the parallax layer.
        ///
        /// The actual rendering implementation depends on the layer content.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public override void Draw(SceneDrawContext context)
        {
            // Rendering will be implemented by derived layer types.
        }
    }
}