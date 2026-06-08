using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics.Effects
{
    /// <summary>
    /// Wraps a MonoGame <see cref="BasicEffect"/> and implements <see cref="IEffectAdapter"/>.
    /// Provides access to camera matrices, texture, color, opacity, and allows matrix copying between transforms.
    /// The effect is lazy-created; GraphicsDevice must be set before first use.
    /// </summary>
    public class BasicEffectAdapter : IEffectAdapter
    {
        private BasicEffect? _effect; // // Interner Effekt, wird lazy erzeugt

        // // Speicherung von Parametern, bevor der Effekt existiert
        private Color _color = new Color(Vector3.One);
        private float _opacity = 1f;

        /// <summary>
        /// The graphics device used to create the underlying BasicEffect.
        /// Must be set before first access to <see cref="Effect"/>.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; } = null!; // // garantiert später gesetzt

        /// <summary>
        /// Gets or sets the base color applied by this effect.
        /// Updates the effect immediately if it exists.
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                if (_effect != null) _effect.DiffuseColor = _color.ToVector3(); // // sofort anwenden
            }
        }

        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by this effect.
        /// Updates the effect immediately if it exists.
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = MathHelper.Clamp(value, 0f, 1f);
                if (_effect != null) _effect.Alpha = _opacity; // // sofort anwenden
            }
        }

        /// <summary>
        /// Gets or sets the texture used by the effect.
        /// Updates the effect immediately if it exists.
        /// </summary>
        public Texture2D? Texture
        {
            get => _effect?.Texture;
            set
            {
                if (_effect != null) _effect.Texture = value; // // direkt anwenden
            }
        }

        /// <summary>
        /// Gets or sets the current technique of the effect.
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get => Effect.CurrentTechnique;
            set => Effect.CurrentTechnique = value;
        }

        // // Kameramatrizen
        public Matrix Projection { get; set; } = Matrix.Identity;
        public Matrix View { get; set; } = Matrix.Identity;
        public Matrix World { get; set; } = Matrix.Identity;

        /// <summary>
        /// Lazy-created BasicEffect. GraphicsDevice must be set before first use.
        /// </summary>
        public BasicEffect Effect
        {
            get
            {
                if (_effect == null)
                {
                    if (GraphicsDevice == null)
                        throw new InvalidOperationException("GraphicsDevice must be set before creating BasicEffect.");

                    _effect = new BasicEffect(GraphicsDevice)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = true,
                        DiffuseColor = _color.ToVector3(),
                        Alpha = _opacity
                    };
                }
                return _effect;
            }
        }

        Effect IEffectAdapter.Effect => Effect;

        /// <summary>
        /// Applies Projection, View, World, Color, Opacity, and Texture to the BasicEffect.
        /// </summary>
        public void Apply()
        {
            var e = Effect;
            e.Projection = Projection;
            e.View = View;
            e.World = World;
            e.DiffuseColor = _color.ToVector3();
            e.Alpha = _opacity;
            e.Texture = Texture;
        }

        /// <summary>
        /// Copies matrices from another camera transform to this adapter.
        /// </summary>
        public void ApplyFrom(ICameraTransform source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Projection = source.Projection;
            View = source.View;
            World = source.World;
        }

        /// <summary>
        /// Copies matrices from this adapter to another camera transform.
        /// </summary>
        public void CopyTo(ICameraTransform target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            target.Projection = Projection;
            target.View = View;
            target.World = World;
        }

        /// <summary>
        /// Disposes the underlying BasicEffect.
        /// </summary>
        public void Dispose()
        {
            _effect?.Dispose();
            _effect = null;
        }

        /// <summary>
        /// Creates a shallow copy of this adapter with current parameters.
        /// </summary>
        public BasicEffectAdapter Clone()
        {
            var clone = new BasicEffectAdapter
            {
                GraphicsDevice = GraphicsDevice,
                Projection = Projection,
                View = View,
                World = World,
                _color = _color,
                _opacity = _opacity,
                Texture = Texture
            };
            return clone;
        }

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Allows implicit conversion from adapter to <see cref="Effect"/>.
        /// Automatically applies all parameters before returning.
        /// </summary>
        public static implicit operator Effect(BasicEffectAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            adapter.Apply(); // // alle Parameter werden übertragen
            return adapter.Effect;
        }
    }
}