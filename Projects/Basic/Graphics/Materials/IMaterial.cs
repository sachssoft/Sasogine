using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Materials
{
    /// <summary>
    /// Defines a material used for configuring shader-based rendering.
    /// A material provides the shader and applies its required rendering data
    /// such as textures and shader parameters.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Gets the shader used by this material for rendering.
        /// </summary>
        IShader Shader { get; }

        /// <summary>
        /// Applies the material settings to the associated shader.
        /// </summary>
        void Apply();
    }
}