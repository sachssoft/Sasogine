using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Defines a common adapter interface for wrapping and controlling a MonoGame <see cref="Effect"/>.
    /// Provides access to transformation matrices, texture, color, opacity and techniques.
    /// </summary>
    public interface IEffectAdapter : IDisposable, ICloneable, ICameraTransform
    {
        /// <summary>
        /// Gets or sets the texture used by the effect.
        /// </summary>
        Texture2D? Texture { get; set; }

        /// <summary>
        /// Gets or sets the current effect technique.
        /// </summary>
        EffectTechnique CurrentTechnique { get; set; }

        /// <summary>
        /// Gets or sets the base color applied by the effect.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Gets or sets the opacity factor (0..1).
        /// </summary>
        float Opacity { get; set; }

        /// <summary>
        /// Applies the configured effect parameters to the inner effect.
        /// </summary>
        void Apply();

        /// <summary>
        /// Gets the wrapped <see cref="Effect"/> instance.
        /// </summary>
        Effect InnerEffect { get; }
    }
}
