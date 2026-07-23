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
    ///
    /// <para>
    /// The asset supports configurable texture filtering, texture addressing,
    /// optional mipmap generation and optional transform handling through
    /// <see cref="ITransformable"/>.
    /// </para>
    ///
    /// <para>
    /// The created texture is intended to be used by rendering components,
    /// materials and shaders inside the Sasogine rendering pipeline.
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
        /// Gets or sets the graphics device used for texture creation.
        /// </summary>
        public GraphicsDevice? GraphicsDevice { get; set; }

        /// <summary>
        /// Initializes a new empty texture asset.
        /// </summary>
        public Texture2DAsset()
            : base(new Texture2DAssetDefinition())
        {
        }

        /// <summary>
        /// Initializes a new texture asset from an existing definition.
        /// </summary>
        /// <param name="definition">
        /// The asset definition containing texture configuration.
        /// </param>
        public Texture2DAsset(
            Texture2DAssetDefinition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Initializes a new texture asset using the specified graphics device.
        /// </summary>
        public Texture2DAsset(
            GraphicsDevice graphicsDevice)
            : base(new Texture2DAssetDefinition())
        {
            GraphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Initializes a new texture asset using the specified graphics device
        /// and asset definition.
        /// </summary>
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
        public SamplerState CreateSamplerState()
        {
            return new SamplerState
            {
                Filter = _filterMode switch
                {
                    Texture2DFilterMode.Point =>
                        TextureFilter.Point,

                    Texture2DFilterMode.Linear =>
                        TextureFilter.Linear,

                    Texture2DFilterMode.Anisotropic =>
                        TextureFilter.Anisotropic,

                    _ =>
                        TextureFilter.Point
                },

                AddressU = CreateAddressMode(),

                AddressV = CreateAddressMode()
            };
        }



        private TextureAddressMode CreateAddressMode()
        {
            return _addressMode switch
            {
                Texture2DAddressMode.Clamp =>
                    TextureAddressMode.Clamp,

                Texture2DAddressMode.Wrap =>
                    TextureAddressMode.Wrap,

                Texture2DAddressMode.Mirror =>
                    TextureAddressMode.Mirror,

                _ =>
                    TextureAddressMode.Clamp
            };
        }

        /// <summary>
        /// Creates the texture transformation matrix.
        /// </summary>
        /// <returns>
        /// A matrix containing translation, rotation and scaling.
        /// </returns>
        public Matrix CreateTransform()
        {
            if (_transformDirty)
            {
                if (_transformable != null)
                {
                    _transformCache =
                        Matrix.CreateTranslation(
                            new Vector3(
                                -_transformable.Origin,
                                0f))
                        *
                        Matrix.CreateScale(
                            new Vector3(
                                _transformable.Scale,
                                1f))
                        *
                        Matrix.CreateRotationZ(
                            _transformable.Rotation)
                        *
                        Matrix.CreateTranslation(
                            new Vector3(
                                _transformable.Origin,
                                0f))
                        *
                        Matrix.CreateTranslation(
                            new Vector3(
                                _transformable.Translation,
                                0f));
                }
                else
                {
                    _transformCache =
                        Matrix.Identity;
                }


                _transformDirty = false;
            }


            return _transformCache;
        }

        /// <summary>
        /// Builds the runtime texture resource from the supplied stream.
        /// </summary>
        /// <param name="stream">
        /// The source texture stream.
        /// </param>
        /// <returns>
        /// A created <see cref="Texture2D"/> instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no graphics device was assigned.
        /// </exception>
        protected override Texture2D? Build(
            Stream stream)
        {
            if (GraphicsDevice == null)
                throw new InvalidOperationException(
                    $"{nameof(Texture2DAsset)} requires a valid {nameof(GraphicsDevice)} before calling {nameof(Build)}.");


            Texture2D original =
                Texture2D.FromStream(
                    GraphicsDevice,
                    stream);


            if (!Definition.UseMipmaps)
                return original;


            int width = original.Width;
            int height = original.Height;

            int mipLevels =
                (int)MathF.Floor(
                    MathF.Log(
                        Math.Max(width, height),
                        2))
                + 1;


            Texture2D texture =
                new Texture2D(
                    GraphicsDevice,
                    width,
                    height,
                    true,
                    SurfaceFormat.Color);


            Color[] pixels =
                new Color[
                    width * height];


            original.GetData(pixels);

            texture.SetData(
                0,
                null,
                pixels,
                0,
                pixels.Length);


            Texture2D currentLevel =
                original;


            for (int level = 1;
                 level < mipLevels;
                 level++)
            {
                width =
                    Math.Max(
                        width / 2,
                        1);

                height =
                    Math.Max(
                        height / 2,
                        1);


                Texture2D nextLevel =
                    Texture2DScaler.DownscaleBox(
                        GraphicsDevice,
                        currentLevel);


                Color[] mipPixels =
                    new Color[
                        width * height];


                nextLevel.GetData(mipPixels);


                texture.SetData(
                    level,
                    null,
                    mipPixels,
                    0,
                    mipPixels.Length);


                currentLevel =
                    nextLevel;
            }


            return texture;
        }

        /// <summary>
        /// Applies configuration values from the asset definition.
        /// </summary>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();


            _filterMode =
                Definition.FilterMode;


            _addressMode =
                Definition.AddressMode;


            if (Definition is ITransformable transformable)
            {
                _transformable =
                    transformable;

                _transformDirty =
                    true;
            }
        }
    }
}