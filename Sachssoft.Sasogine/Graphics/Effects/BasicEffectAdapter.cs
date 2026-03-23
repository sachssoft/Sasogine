using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Wraps a MonoGame <see cref="BasicEffect"/> and implements <see cref="IEffectAdapter"/>.
    /// Provides access to camera matrices, texture, color, opacity, and allows matrix copying between transforms.
    /// </summary>
    public class BasicEffectAdapter : BasicEffect, IEffectAdapter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BasicEffectAdapter"/> using the current graphics device.
        /// </summary>
        public BasicEffectAdapter()
            : this(IGameApplication.Current.GraphicsDevice)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BasicEffectAdapter"/> using the specified graphics device.
        /// Enables vertex coloring and textures by default.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        public BasicEffectAdapter(GraphicsDevice device)
            : base(device)
        {
            VertexColorEnabled = true;
            TextureEnabled = true;
            Opacity = 1f;
        }

        /// <summary>
        /// Gets or sets the base color applied by this effect.
        /// </summary>
        public Color Color
        {
            get => new Color(DiffuseColor);
            set => DiffuseColor = value.ToVector3();
        }

        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by this effect.
        /// </summary>
        public float Opacity
        {
            get => Alpha;
            set => Alpha = value;
        }

        /// <summary>
        /// Gets the wrapped <see cref="Effect"/> instance.
        /// </summary>
        Effect IEffectAdapter.InnerEffect => this;

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from another transform provider to this effect.
        /// </summary>
        /// <param name="source">The source camera transform providing the matrices.</param>
        public void ApplyFrom(ICameraTransform source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Projection = source.Projection;
            View = source.View;
            World = source.World;
        }

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from this effect to another transform provider.
        /// </summary>
        /// <param name="target">The target camera transform to receive the matrices.</param>
        public void CopyTo(ICameraTransform target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.Projection = Projection;
            target.View = View;
            target.World = World;
        }

        /// <summary>
        /// Applies the configured effect parameters to the underlying <see cref="BasicEffect"/>.
        /// Override in derived classes to bind additional matrices, textures, or other parameters.
        /// </summary>
        void IEffectAdapter.Apply()
        {
            // No default implementation. Derived classes can override.
        }

        /// <summary>
        /// Creates a copy of this adapter.
        /// </summary>
        /// <returns>A new <see cref="BasicEffectAdapter"/> instance with the same configuration.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
