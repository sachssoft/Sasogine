using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Defines a common adapter interface for wrapping and controlling a MonoGame <see cref="Effect"/>.
    /// Provides access to transformation matrices, texture, color, opacity, and techniques.
    /// </summary>
    public interface IEffectAdapter : IDisposable, ICloneable, ICameraTransform
    {
        /// <summary>
        /// The graphics device used to create the underlying effect.
        /// Must be set before the effect is created.
        /// </summary>
        GraphicsDevice GraphicsDevice { get; set; } // muss vor Effekt-Erstellung gesetzt werden

        /// <summary>
        /// Gets or sets the texture used by the effect.
        /// </summary>
        Texture2D? Texture { get; set; } // // die Textur, die auf den Effekt angewendet wird

        /// <summary>
        /// Gets or sets the current effect technique.
        /// </summary>
        EffectTechnique CurrentTechnique { get; set; } // // die aktuell verwendete Technique des Effekts

        /// <summary>
        /// Gets or sets the base color applied by the effect.
        /// </summary>
        Color Color { get; set; } // // Grundfarbe, die auf den Effekt angewendet wird

        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by the effect.
        /// </summary>
        float Opacity { get; set; } // // Transparenzfaktor für den Effekt

        /// <summary>
        /// Applies the configured effect parameters to the inner effect.
        /// </summary>
        void Apply(); // // wendet alle aktuellen Parameter auf den InnerEffect an

        /// <summary>
        /// Gets the underlying MonoGame <see cref="Effect"/>.
        /// </summary>
        Effect Effect { get; } // // der zugrunde liegende Effekt
    }
}