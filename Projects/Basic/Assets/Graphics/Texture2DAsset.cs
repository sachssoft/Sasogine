using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Represents a managed 2D texture asset for the Sasogine graphics system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Texture2DAsset"/> is responsible for loading and configuring
    /// <see cref="Texture2D"/> resources from asset streams.
    /// </para>
    /// <para>
    /// The asset supports configurable texture filtering, texture addressing,
    /// optional mipmap generation, and optional transform handling through
    /// <see cref="ITransformable"/>.
    /// </para>
    /// <para>
    /// A valid <see cref="GraphicsDevice"/> must be assigned before the runtime
    /// texture resource can be created.
    /// </para>
    /// </remarks>
    public class Texture2DAsset :
        AssetBase<Texture2D, Texture2DAssetDefinition>
    {
        private ITransformable? _transformable;
        private Texture2DFilterMode _filterMode;
        private Texture2DAddressMode _addressMode;
        private Matrix _transformCache = Matrix.Identity;

        private bool _transformDirty = true;

        /// <summary>
        /// Gets or sets the graphics device used to create texture resources.
        /// </summary>
        /// <value>
        /// The graphics device, or <see langword="null"/> if no graphics device
        /// has been assigned.
        /// </value>
        public GraphicsDevice? GraphicsDevice { get; set; }

        /// <summary>
        /// Initializes a new empty instance of the
        /// <see cref="Texture2DAsset"/> class.
        /// </summary>
        /// <param name="id">
        /// The optional identifier of the asset.
        /// </param>
        /// <param name="class">
        /// The optional class of the asset.
        /// </param>
        public Texture2DAsset(
            string? id = null,
            string? @class = null)
            : base(new Texture2DAssetDefinition
            {
                Id = id,
                Class = @class,
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Texture2DAsset"/> class from an existing definition.
        /// </summary>
        /// <param name="definition">
        /// The asset definition containing the texture configuration.
        /// </param>
        public Texture2DAsset(
            Texture2DAssetDefinition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Texture2DAsset"/> class using the specified graphics device.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used to create texture resources.
        /// </param>
        /// <param name="id">
        /// The optional identifier of the asset.
        /// </param>
        /// <param name="class">
        /// The optional class of the asset.
        /// </param>
        public Texture2DAsset(
            GraphicsDevice graphicsDevice,
            string? id = null,
            string? @class = null)
            : this(id, @class)
        {
            GraphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Texture2DAsset"/> class using the specified graphics device
        /// and asset definition.
        /// </summary>
        /// <param name="graphicsDevice">
        /// The graphics device used to create texture resources.
        /// </param>
        /// <param name="definition">
        /// The asset definition containing the texture configuration.
        /// </param>
        public Texture2DAsset(
            GraphicsDevice graphicsDevice,
            Texture2DAssetDefinition definition)
            : base(definition)
        {
            GraphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Creates a sampler state based on the configured filter and address modes.
        /// </summary>
        /// <returns>
        /// A configured <see cref="SamplerState"/>.
        /// </returns>
        /// <remarks>
        /// The configured address mode is applied to both horizontal and vertical
        /// texture coordinates.
        /// </remarks>
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
        /// Creates the texture transformation matrix.
        /// </summary>
        /// <returns>
        /// A matrix containing the configured origin, scale, rotation, and
        /// translation.
        /// </returns>
        /// <remarks>
        /// The transformation matrix is cached until the transform configuration
        /// changes. If no transformation is configured,
        /// <see cref="Matrix.Identity"/> is returned.
        /// </remarks>
        public Matrix CreateTransform()
        {
            if (_transformDirty)
            {
                if (_transformable != null)
                {
                    _transformCache =
                        Matrix.CreateTranslation(new Vector3(-_transformable.Origin, 0f))
                        * Matrix.CreateScale(new Vector3(_transformable.Scale, 1f))
                        * Matrix.CreateRotationZ(_transformable.Rotation)
                        * Matrix.CreateTranslation(new Vector3(_transformable.Origin, 0f))
                        * Matrix.CreateTranslation(new Vector3(_transformable.Translation, 0f));
                }
                else
                {
                    _transformCache = Matrix.Identity;
                }

                _transformDirty = false;
            }

            return _transformCache;
        }

        /// <summary>
        /// Builds the runtime texture resource from the supplied stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the source texture data.
        /// </param>
        /// <returns>
        /// The created <see cref="Texture2D"/> instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// No <see cref="GraphicsDevice"/> has been assigned.
        /// </exception>
        /// <remarks>
        /// If mipmaps are disabled, the texture loaded directly from the stream
        /// is returned. Otherwise, additional mip levels are generated by
        /// successively downscaling the source texture.
        /// </remarks>
        protected override Texture2D? Build(
            Stream stream)
        {
            if (GraphicsDevice == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(Texture2DAsset)} requires a valid {nameof(GraphicsDevice)} before calling {nameof(Build)}.");
            }

            Texture2D original = Texture2D.FromStream(GraphicsDevice, stream);

            if (!Definition.UseMipmaps)
                return original;

            int width = original.Width;
            int height = original.Height;

            int mipLevels = (int)MathF.Floor(
                MathF.Log(Math.Max(width, height), 2)) + 1;

            Texture2D texture = new Texture2D(
                GraphicsDevice,
                width,
                height,
                true,
                SurfaceFormat.Color);

            Color[] pixels = new Color[width * height];

            original.GetData(pixels);
            texture.SetData(0, null, pixels, 0, pixels.Length);

            Texture2D currentLevel = original;

            for (int level = 1; level < mipLevels; level++)
            {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);

                Texture2D nextLevel =
                    Texture2DScaler.DownscaleBox(GraphicsDevice, currentLevel);

                Color[] mipPixels = new Color[width * height];

                nextLevel.GetData(mipPixels);
                texture.SetData(level, null, mipPixels, 0, mipPixels.Length);

                currentLevel = nextLevel;
            }

            return texture;
        }

        /// <summary>
        /// Applies configuration values from the current asset definition.
        /// </summary>
        /// <remarks>
        /// Updates the texture filter, address mode, and optional transformation
        /// configuration. The cached transformation matrix is invalidated when
        /// the definition is configured.
        /// </remarks>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            _filterMode = Definition.FilterMode;
            _addressMode = Definition.AddressMode;
            _transformable = Definition as ITransformable;
            _transformDirty = true;
        }

        /// <summary>
        /// Converts the configured texture address mode to the corresponding
        /// MonoGame texture address mode.
        /// </summary>
        /// <returns>
        /// The corresponding <see cref="TextureAddressMode"/>.
        /// </returns>
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