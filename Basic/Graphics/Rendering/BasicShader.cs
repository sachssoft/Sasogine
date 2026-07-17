using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Provides a shader implementation based on MonoGame's <see cref="BasicEffect"/>.
    /// Implements <see cref="IShader"/> and <see cref="IShaderTransform"/>.
    /// 
    /// Manages shader state such as camera transformation, world transformation,
    /// texture, color, and opacity.
    ///
    /// The underlying effect is created lazily. A valid <see cref="GraphicsDevice"/>
    /// must be assigned before the first access to <see cref="Effect"/>.
    /// Shader properties only store the current state; changes are applied to the
    /// underlying effect when <see cref="Apply"/> is called.
    /// </summary>
    public class BasicShader : IShader, IShaderTransform
    {
        private BasicEffect? _effect;

        private Color _color = Color.White;
        private float _opacity = 1f;
        private Texture2D? _texture; 
        private ICameraTransform? _camera;
        private Matrix _transform = Matrix.Identity;

        /// <summary>
        /// The graphics device used to create the underlying BasicEffect.
        /// Must be assigned before first access to <see cref="Effect"/>.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; set; } = null!;


        /// <summary>
        /// Gets or sets the base color applied by this shader.
        /// The value is applied when <see cref="Apply"/> is called.
        /// </summary>
        public Color Color
        {
            get => _color;
            set => _color = value;
        }


        /// <summary>
        /// Gets or sets the opacity factor (0..1) applied by this shader.
        /// The value is applied when <see cref="Apply"/> is called.
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set => _opacity = MathHelper.Clamp(value, 0f, 1f);
        }


        /// <summary>
        /// Gets or sets the texture used by this shader.
        /// The value is applied when <see cref="Apply"/> is called.
        /// </summary>
        public Texture2D? Texture
        {
            get => _texture;
            set => _texture = value;
        }


        /// <summary>
        /// Gets or sets the camera transformation used by this shader.
        /// Camera matrices are read when <see cref="Apply"/> is called.
        /// </summary>
        public ICameraTransform? Camera
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
        /// Gets or sets the current technique of the underlying effect.
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get => Effect.CurrentTechnique;
            set => Effect.CurrentTechnique = value;
        }


        /// <summary>
        /// Gets the underlying MonoGame <see cref="BasicEffect"/> instance.
        /// The effect is created lazily on first access.
        /// </summary>
        public BasicEffect Effect
        {
            get
            {
                if (_effect == null)
                {
                    if (GraphicsDevice == null)
                    {
                        throw new InvalidOperationException(
                            "GraphicsDevice must be assigned before creating BasicEffect.");
                    }

                    _effect = new BasicEffect(GraphicsDevice)
                    {
                        VertexColorEnabled = true,
                        TextureEnabled = _texture != null,
                        Texture = _texture,
                        DiffuseColor = _color.ToVector3(),
                        Alpha = _opacity
                    };
                }

                return _effect;
            }
        }


        /// <summary>
        /// Gets the underlying MonoGame <see cref="Effect"/> instance.
        /// </summary>
        Effect IShader.Effect => Effect;


        /// <summary>
        /// Applies the current shader state, including transformation,
        /// camera, texture, color, and opacity, to the underlying effect.
        /// </summary>
        public void Apply()
        {
            var effect = Effect;

            if (_camera != null)
            {
                effect.World = _camera.World * _transform;
                effect.View = _camera.View;
                effect.Projection = _camera.Projection;
            }
            else
            {
                effect.World = _transform;
            }

            effect.DiffuseColor = _color.ToVector3();
            effect.Alpha = _opacity;

            effect.Texture = _texture;
            effect.TextureEnabled = _texture != null;
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
        /// Creates a copy of this shader with the current configuration.
        /// </summary>
        public BasicShader Clone()
        {
            return new BasicShader
            {
                GraphicsDevice = GraphicsDevice,
                Camera = Camera,
                Transform = Transform,
                Color = Color,
                Opacity = Opacity,
                Texture = Texture
            };
        }


        object ICloneable.Clone() => Clone();


        /// <summary>
        /// Converts this shader to a MonoGame <see cref="Effect"/> instance.
        /// The shader state is applied before returning the effect.
        /// </summary>
        public static implicit operator Effect(BasicShader shader)
        {
            ArgumentNullException.ThrowIfNull(shader);

            shader.Apply();

            return shader.Effect;
        }
    }
}