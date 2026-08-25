using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines a shader abstraction for controlling and configuring a MonoGame <see cref="Effect"/>.
    /// Provides access to effect parameters such as techniques, textures, colors, opacity,
    /// and allows applying the configured shader state.
    /// This abstraction is designed to support future backend-independent rendering versions.
    /// </summary>
    public interface IShader : IDisposable, ICloneable
    {
        /// <summary>
        /// The graphics device used to create the underlying MonoGame effect.
        /// Must be assigned before the effect is created.
        /// </summary>
        GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Gets or sets the current technique of the underlying effect.
        /// </summary>
        EffectTechnique CurrentTechnique { get; set; }

        /// <summary>
        /// Gets or sets the texture applied by the shader.
        /// </summary>
        Texture2D? Texture { get; set; }

        /// <summary>
        /// Gets or sets the base color applied by the shader.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by the shader.
        /// </summary>
        float Opacity { get; set; }

        /// <summary>
        /// Applies the configured shader parameters to the underlying effect.
        /// </summary>
        void Apply();

        /// <summary>
        /// Gets the underlying MonoGame <see cref="Effect"/> instance.
        /// </summary>
        Effect Effect { get; }
    }
}