using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Common;
using Sachssoft.Engine.Graphics;
using Sachssoft.Engine.Resources;
using System;
using System.IO;

namespace Sachssoft.Engine.Assets.Graphics
{
    /// <summary>
    /// Represents a managed 2D texture asset for the Sasogine graphics system.
    /// </summary>
    /// <remarks>
    /// Loads and configures <see cref="Texture2D"/> resources and supports
    /// filtering, addressing, mipmaps, and optional transformations.
    /// </remarks>
    public class Texture2DAsset : AssetBase<Texture2D, Texture2DAssetDefinition>
    {
        private ITransform2? _transform;
        private Texture2DFilterMode _filterMode;
        private Texture2DAddressMode _addressMode;
        private Matrix _transformCache = Matrix.Identity;
        private bool _transformDirty = true;

        /// <summary>
        /// Initializes a new texture asset.
        /// </summary>
        /// <param name="id">The optional asset identifier.</param>
        public Texture2DAsset(string? id = null)
            : base(new Texture2DAssetDefinition { Id = id })
        {
        }

        /// <summary>
        /// Initializes a new texture asset using the specified definition.
        /// </summary>
        /// <param name="definition">The texture asset definition.</param>
        public Texture2DAsset(Texture2DAssetDefinition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Initializes a new texture asset with the specified resource source.
        /// </summary>
        /// <param name="id">The optional asset identifier.</param>
        /// <param name="loaderSource">The resource source used to load the texture.</param>
        public Texture2DAsset(string? id, ResourceSourceBase? loaderSource)
            : base(new Texture2DAssetDefinition { Id = id })
        {
            LoaderSource = loaderSource;
        }

        /// <summary>
        /// Initializes a new texture asset using the specified definition and resource source.
        /// </summary>
        /// <param name="definition">The texture asset definition.</param>
        /// <param name="loaderSource">The resource source used to load the texture.</param>
        public Texture2DAsset(Texture2DAssetDefinition definition, ResourceSourceBase? loaderSource)
            : base(definition)
        {
            LoaderSource = loaderSource;
        }

        /// <summary>
        /// Creates a sampler state using the configured filter and address modes.
        /// </summary>
        /// <returns>The configured sampler state.</returns>
        public SamplerState CreateSamplerState()
        {
            return new SamplerState
            {
                Filter = _filterMode switch
                {
                    Texture2DFilterMode.Point => TextureFilter.Point,
                    Texture2DFilterMode.Linear => TextureFilter.Linear,
                    Texture2DFilterMode.Anisotropic => TextureFilter.Anisotropic,
                    _ => TextureFilter.Point
                },
                AddressU = CreateAddressMode(),
                AddressV = CreateAddressMode()
            };
        }

        /// <summary>
        /// Creates the configured texture transformation matrix.
        /// </summary>
        /// <returns>The transformation matrix, or <see cref="Matrix.Identity"/> if no transform exists.</returns>
        public Matrix CreateTransform()
        {
            if (!_transformDirty)
                return _transformCache;

            if (_transform is null)
            {
                _transformCache = Matrix.Identity;
                _transformDirty = false;
                return _transformCache;
            }

            Point2 position = _transform is IReadOnlyTransformPosition2 p ? p.Position : Point2.Zero;
            Vector2 scale = _transform is IReadOnlyTransformScale2 s ? s.Scale : Vector2.One;
            float rotation = _transform is IReadOnlyTransformRotation2 r ? r.Rotation : 0f;
            Point2 pivot = _transform is IReadOnlyTransformRotationPivot2 rp ? rp.RotationPivot : Point2.Zero;

            _transformCache =
                Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f) *
                Matrix.CreateScale(scale.X, scale.Y, 1f) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateTranslation(pivot.X, pivot.Y, 0f) *
                Matrix.CreateTranslation(position.X, position.Y, 0f);

            _transformDirty = false;
            return _transformCache;
        }

        /// <summary>
        /// Builds the runtime texture from the supplied stream.
        /// </summary>
        /// <param name="stream">The stream containing the texture data.</param>
        /// <returns>The created runtime texture.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The asset has not been initialized.</exception>
        protected override Texture2D Build(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var graphicsDevice = Context?.GraphicsDevice ??
                throw new InvalidOperationException(
                    $"{nameof(Texture2DAsset)} must be initialized before calling {nameof(Build)}.");

            var original = Texture2D.FromStream(graphicsDevice, stream);

            if (!Definition.UseMipmaps)
                return original;

            int width = original.Width;
            int height = original.Height;
            int mipLevels = (int)MathF.Floor(MathF.Log(Math.Max(width, height), 2)) + 1;

            Texture2D texture = new(graphicsDevice, width, height, true, SurfaceFormat.Color);
            Color[] pixels = new Color[width * height];

            original.GetData(pixels);
            texture.SetData(0, null, pixels, 0, pixels.Length);

            Texture2D currentLevel = original;

            for (int level = 1; level < mipLevels; level++)
            {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);

                Texture2D nextLevel = Texture2DScaler.DownscaleBox(graphicsDevice, currentLevel);
                Color[] mipPixels = new Color[width * height];

                nextLevel.GetData(mipPixels);
                texture.SetData(level, null, mipPixels, 0, mipPixels.Length);

                if (!ReferenceEquals(currentLevel, original))
                    currentLevel.Dispose();

                currentLevel = nextLevel;
            }

            if (!ReferenceEquals(currentLevel, original))
                currentLevel.Dispose();

            original.Dispose();
            return texture;
        }

        /// <summary>
        /// Applies the current texture asset definition.
        /// </summary>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            _filterMode = Definition.FilterMode;
            _addressMode = Definition.AddressMode;
            _transform = Definition as ITransform2;
            _transformDirty = true;
        }

        /// <summary>
        /// Converts the configured address mode to a MonoGame texture address mode.
        /// </summary>
        /// <returns>The corresponding texture address mode.</returns>
        private TextureAddressMode CreateAddressMode()
        {
            return _addressMode switch
            {
                Texture2DAddressMode.Clamp => TextureAddressMode.Clamp,
                Texture2DAddressMode.Wrap => TextureAddressMode.Wrap,
                Texture2DAddressMode.Mirror => TextureAddressMode.Mirror,
                _ => TextureAddressMode.Clamp
            };
        }
    }
}