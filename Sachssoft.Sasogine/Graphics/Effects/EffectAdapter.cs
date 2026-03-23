using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Base class for wrapping and adapting a MonoGame <see cref="Effect"/>.
    /// Provides common properties such as camera matrices, texture, color, opacity, and effect techniques.
    /// </summary>
    public abstract class EffectAdapter : IEffectAdapter, IDisposable, ICloneable
    {
        private readonly Effect _effect;

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectAdapter"/> class.
        /// </summary>
        /// <param name="effect">The underlying MonoGame effect.</param>
        protected EffectAdapter(Effect effect)
        {
            _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }

        /// <summary>
        /// Gets the wrapped <see cref="Effect"/> instance.
        /// </summary>
        public Effect InnerEffect => _effect;

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public virtual Matrix Projection { get; set; } = Matrix.Identity;

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public virtual Matrix View { get; set; } = Matrix.Identity;

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public virtual Matrix World { get; set; } = Matrix.Identity;

        /// <summary>
        /// Gets or sets the texture used by this effect.
        /// </summary>
        public virtual Texture2D? Texture { get; set; } = null;

        /// <summary>
        /// Gets or sets the base color.
        /// </summary>
        public virtual Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the opacity factor (0..1).
        /// </summary>
        public virtual float Opacity { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the current effect technique.
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get => _effect.CurrentTechnique;
            set => _effect.CurrentTechnique = value;
        }

        /// <summary>
        /// Creates a copy of this adapter with the same configuration.
        /// </summary>
        /// <returns>A new <see cref="EffectAdapter"/> instance.</returns>
        public abstract EffectAdapter Clone();

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Applies the configured effect parameters to the inner effect.
        /// Override in derived classes to bind matrices, texture, color, etc.
        /// </summary>
        public virtual void Apply()
        {
            // Example: derived classes can set shader parameters here
            // e.g., _effect.Parameters["WorldViewProjection"]?.SetValue(World * View * Projection);
        }

        /// <summary>
        /// Releases the underlying effect and associated resources.
        /// </summary>
        public virtual void Dispose()
        {
            _effect.Dispose();
        }

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from another transform provider to this instance.
        /// </summary>
        /// <param name="source">The source camera transform.</param>
        public void ApplyFrom(ICameraTransform source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Projection = source.Projection;
            View = source.View;
            World = source.World;
        }

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from this instance to another transform provider.
        /// </summary>
        /// <param name="target">The target camera transform.</param>
        public void CopyTo(ICameraTransform target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.Projection = Projection;
            target.View = View;
            target.World = World;
        }
    }
}
