using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics.Effects;
using System;

namespace Sachssoft.Sasogine.Graphics.Effects
{
    /// <summary>
    /// Generic adapter for wrapping and controlling any MonoGame <see cref="Effect"/>.
    /// Provides lazy initialization, automatic parameter application, and camera matrix handling.
    /// </summary>
    public sealed class TemplatedEffectAdapter<TEffect> : IEffectAdapter
        where TEffect : Effect
    {
        private readonly Func<GraphicsDevice, TEffect> _effectFactory;
        private readonly Func<TEffect, TemplatedEffectParameter, object?> _getter;
        private readonly Action<TEffect, TemplatedEffectParameter, object?> _setter;
        private TEffect? _effect; // // Interner Effekt, wird lazy erzeugt

        // // Speicherung von Parametern vor Effekt-Erzeugung
        private Color _color = new Color(Vector3.One);
        private float _opacity = 1f;
        private Texture2D? _texture;

        /// <summary>
        /// Creates a new templated effect adapter.
        /// </summary>
        /// <param name="effectFactory">Factory to create the underlying effect.</param>
        /// <param name="getter">Getter for effect parameters.</param>
        /// <param name="setter">Setter for effect parameters.</param>
        public TemplatedEffectAdapter(
            Func<GraphicsDevice, TEffect> effectFactory,
            Func<TEffect, TemplatedEffectParameter, object?> getter,
            Action<TEffect, TemplatedEffectParameter, object?> setter)
        {
            _effectFactory = effectFactory ?? throw new ArgumentNullException(nameof(effectFactory));
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// The graphics device used to create the underlying effect.
        /// Must be set before first access to <see cref="Effect"/>.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; } = null!; // // init-only, muss vor Effekt-Erzeugung gesetzt werden

        /// <summary>
        /// Gets or sets the base color applied to the effect.
        /// Automatically updates the effect if it has been created.
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                if (_effect != null)
                    _setter(_effect, TemplatedEffectParameter.Color, value);
            }
        }

        /// <summary>
        /// Gets or sets the opacity factor (0..1).
        /// Automatically updates the effect if it exists.
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = float.Clamp(value, 0f, 1f);
                if (_effect != null)
                    _setter(_effect, TemplatedEffectParameter.Opacity, _opacity);
            }
        }

        /// <summary>
        /// Gets or sets the texture applied to the effect.
        /// Automatically updates the effect if it exists.
        /// </summary>
        public Texture2D? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (_effect != null)
                    _setter(_effect, TemplatedEffectParameter.Texture, value);
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
        /// Gets the underlying effect. Lazy-creates it on first access.
        /// </summary>
        public TEffect Effect
        {
            get
            {
                if (_effect == null)
                {
                    if (GraphicsDevice == null)
                        throw new InvalidOperationException("GraphicsDevice must be set before creating the effect.");
                    _effect = _effectFactory(GraphicsDevice);
                    // // Parameter vor Anwendung übernehmen
                    Apply();
                }
                return _effect;
            }
        }

        Effect IEffectAdapter.Effect => Effect;

        /// <summary>
        /// Applies all stored parameters and matrices to the underlying effect.
        /// </summary>
        public void Apply()
        {
            if (_effect == null)
                throw new InvalidOperationException("Effect has not been created yet.");

            _setter(_effect, TemplatedEffectParameter.Projection, Projection);
            _setter(_effect, TemplatedEffectParameter.View, View);
            _setter(_effect, TemplatedEffectParameter.World, World);
            _setter(_effect, TemplatedEffectParameter.Color, Color);
            _setter(_effect, TemplatedEffectParameter.Opacity, Opacity);
            _setter(_effect, TemplatedEffectParameter.Texture, Texture);
        }

        /// <summary>
        /// Copies matrices from another camera transform into this adapter.
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
        /// Disposes the underlying effect.
        /// </summary>
        public void Dispose()
        {
            _effect?.Dispose();
            _effect = null;
        }

        /// <summary>
        /// Creates a shallow copy of this adapter with all current parameters.
        /// </summary>
        public TemplatedEffectAdapter<TEffect> Clone()
        {
            var cloned = new TemplatedEffectAdapter<TEffect>(_effectFactory, _getter, _setter)
            {
                _color = _color,
                _opacity = _opacity,
                _texture = _texture,
                Projection = Projection,
                View = View,
                World = World
            };
            cloned.Apply();
            return cloned;
        }

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Implicitly converts this adapter to <see cref="Effect"/>.
        /// All stored parameters are applied before returning the effect.
        /// </summary>
        public static implicit operator Effect(TemplatedEffectAdapter<TEffect> adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            adapter.Apply(); // // alle Parameter werden übernommen
            return adapter.Effect;
        }
    }
}