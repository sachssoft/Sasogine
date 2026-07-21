using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Materials
{
    /// <summary>
    /// Defines a basic material that renders geometry using a diffuse shader.
    /// Supports a color tint and an optional texture.
    /// </summary>
    public sealed class DiffuseMaterial : IMaterial
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiffuseMaterial"/> class.
        /// </summary>
        /// <param name="shader">
        /// The shader used for rendering this material.
        /// </param>
        public DiffuseMaterial(IShader shader)
        {
            Shader = shader;
        }

        /// <summary>
        /// Gets the shader used by this material.
        /// </summary>
        public IShader Shader { get; }

        /// <summary>
        /// Gets or sets the diffuse color applied during rendering.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the optional diffuse texture.
        /// </summary>
        public Texture2D? Texture { get; set; }

        /// <summary>
        /// Applies the material properties to the associated shader.
        /// </summary>
        public void Apply()
        {
            Shader.Color = Color;
            Shader.Texture = Texture;
        }
    }
}