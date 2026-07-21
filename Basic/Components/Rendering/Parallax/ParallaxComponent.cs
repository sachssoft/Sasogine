using Sachssoft.Sasogine.Scenes;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    /// <summary>
    /// Provides a parallax rendering component that manages multiple
    /// independent parallax layers.
    ///
    /// Each layer can update and render individually while the component
    /// controls layer ordering and visibility.
    /// </summary>
    public class ParallaxComponent : ResourceComponentBase<ParallaxDefinition>, IDrawableComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParallaxComponent"/> class
        /// using a default parallax definition.
        /// </summary>
        public ParallaxComponent() : this(new ParallaxDefinition())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ParallaxComponent"/> class.
        /// </summary>
        /// <param name="parallaxDefinition">
        /// The definition containing parallax configuration data.
        /// </param>
        public ParallaxComponent(ParallaxDefinition parallaxDefinition) : base(parallaxDefinition)
        {
        }


        /// <summary>
        /// Gets the collection of parallax layers managed by this component.
        /// Layers are rendered according to their collection order.
        /// </summary>
        public List<IParallaxLayerComponent> Layers { get; } = new List<IParallaxLayerComponent>();


        /// <summary>
        /// Gets or sets whether the parallax component is rendered.
        /// </summary>
        public bool IsVisible { get; set; }


        /// <summary>
        /// Creates a default parallax definition when no definition is available.
        /// </summary>
        /// <returns>
        /// A new default <see cref="ParallaxDefinition"/> instance.
        /// </returns>
        protected override ParallaxDefinition ResolveDefinition()
        {
            return new ParallaxDefinition();
        }


        /// <summary>
        /// Updates all enabled parallax layers and assigns their draw order.
        /// </summary>
        /// <param name="context">
        /// Provides scene update information.
        /// </param>
        public void Update(SceneUpdateContext context)
        {
        }


        /// <summary>
        /// Draws all visible parallax layers.
        /// </summary>
        /// <param name="context">
        /// Provides scene rendering information.
        /// </param>
        public void Draw(SceneDrawContext context)
        {
        }
    }
}