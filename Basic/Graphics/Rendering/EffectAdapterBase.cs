using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Base class for implementing a shader based on a MonoGame <see cref="Effect"/>.
    /// Provides common shader state such as camera transformation, texture, color,
    /// opacity, and effect technique handling.
    ///
    /// Shader properties only store the current state. Derived classes should apply
    /// the state to the underlying effect inside <see cref="Apply"/>.
    /// </summary>
    public abstract class ShaderBase : IShader, IDisposable, ICloneable
    {
        private readonly Effect _effect;

        private Texture2D? _texture;
        private Color _color = Color.White;
        private float _opacity = 1f;
        private ICameraTransform? _camera;
        private Matrix _transform = Matrix.Identity;


        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderBase"/> class.
        /// </summary>
        /// <param name="effect">The underlying MonoGame effect.</param>
        protected ShaderBase(Effect effect)
        {
            _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }


        /// <summary>
        /// Gets or sets the graphics device used by this shader.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; } = null!;


        /// <summary>
        /// Gets the underlying MonoGame <see cref="Effect"/> instance.
        /// </summary>
        public Effect Effect => _effect;


        /// <summary>
        /// Gets or sets the camera transformation used by this shader.
        /// Camera values are read when <see cref="Apply"/> is called.
        /// </summary>
        public virtual ICameraTransform? Camera
        {
            get => _camera;
            set => _camera = value;
        }

        /// <summary>
        /// Gets or sets the local world transformation matrix used by this shader.
        /// </summary>
        public Matrix Transform
        {
            get => _transform;
            set => _transform = value;
        }


        /// <summary>
        /// Gets or sets the texture used by this shader.
        /// </summary>
        public virtual Texture2D? Texture
        {
            get => _texture;
            set => _texture = value;
        }


        /// <summary>
        /// Gets or sets the base color applied by this shader.
        /// </summary>
        public virtual Color Color
        {
            get => _color;
            set => _color = value;
        }


        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by this shader.
        /// </summary>
        public virtual float Opacity
        {
            get => _opacity;
            set => _opacity = MathHelper.Clamp(value, 0f, 1f);
        }


        /// <summary>
        /// Gets or sets the current effect technique.
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get => _effect.CurrentTechnique;
            set => _effect.CurrentTechnique = value;
        }


        /// <summary>
        /// Creates a copy of this shader with the same configuration.
        /// </summary>
        /// <returns>A new shader instance.</returns>
        public abstract ShaderBase Clone();


        object ICloneable.Clone() => Clone();


        /// <summary>
        /// Applies the current shader state to the underlying effect.
        /// Derived classes should override this method to bind effect-specific parameters.
        /// </summary>
        public virtual void Apply()
        {
            // Derived classes apply shader parameters here.
        }


        /// <summary>
        /// Disposes the underlying effect and releases resources.
        /// </summary>
        public virtual void Dispose()
        {
            _effect.Dispose();
        }
    }
}