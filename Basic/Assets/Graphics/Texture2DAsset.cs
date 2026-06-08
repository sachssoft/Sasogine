using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class Texture2DAsset : AssetBase<Texture2D, Texture2DDefinition>
    {
        private Texture2DFilterMode _filterMode;
        private Texture2DAddressMode _addressMode;
        private Vector2 _translation = Vector2.Zero;
        private float _rotation = 0f;
        private Vector2 _scale = Vector2.One;
        private Vector2 _pivot = Vector2.Zero;
        private Matrix _transformCache = Matrix.Identity;
        private bool _transformDirty = true;

        public GraphicsDevice? GraphicsDevice { get; set; }

        protected override Texture2DDefinition CreateDefinition()
        {
            return new Texture2DDefinition();
        }

        public SamplerState CreateSamplerState()
        {
            var state = new SamplerState
            {
                Filter = _filterMode switch
                {
                    Texture2DFilterMode.Point => TextureFilter.Point,
                    Texture2DFilterMode.Linear => TextureFilter.Linear,
                    Texture2DFilterMode.Anisotropic => TextureFilter.Anisotropic,
                    _ => TextureFilter.Point
                },
                AddressU = _addressMode switch
                {
                    Texture2DAddressMode.Clamp => TextureAddressMode.Clamp,
                    Texture2DAddressMode.Wrap => TextureAddressMode.Wrap,
                    Texture2DAddressMode.Mirror => TextureAddressMode.Mirror,
                    _ => TextureAddressMode.Clamp
                },
                AddressV = _addressMode switch
                {
                    Texture2DAddressMode.Clamp => TextureAddressMode.Clamp,
                    Texture2DAddressMode.Wrap => TextureAddressMode.Wrap,
                    Texture2DAddressMode.Mirror => TextureAddressMode.Mirror,
                    _ => TextureAddressMode.Clamp
                }
            };
            return state;
        }

        public Matrix CreateTransform()
        {
            if (_transformDirty)
            {
                _transformCache =
                    Matrix.CreateTranslation(new Vector3(-_pivot, 0f)) *
                    Matrix.CreateScale(new Vector3(_scale, 1f)) *
                    Matrix.CreateRotationZ(_rotation) *
                    Matrix.CreateTranslation(new Vector3(_pivot, 0f)) *
                    Matrix.CreateTranslation(new Vector3(_translation, 0f));

                _transformDirty = false;
            }

            return _transformCache;
        }

        protected override Texture2D? Build(Stream stream)
        {
            if (GraphicsDevice == null)
                throw new InvalidOperationException($"{nameof(Texture2DAsset)} requires a valid {nameof(GraphicsDevice)} before calling {nameof(Build)}.");

            // 1. Volltextur aus Stream laden
            Texture2D original = Texture2D.FromStream(GraphicsDevice, stream);

            if (!Definition.UseMipmaps)
            {
                // Keine MipMaps, direkt zurückgeben
                return original;
            }

            // 2. Neue Texture mit MipMap-Unterstützung anlegen
            int width = original.Width;
            int height = original.Height;
            int mipLevels = (int)float.Floor(float.Log(int.Max(width, height), 2)) + 1;

            Texture2D texture = new Texture2D(GraphicsDevice, width, height, mipmap: true, SurfaceFormat.Color);

            // 3. Basislevel (Level 0) setzen
            Color[] pixels = new Color[width * height];
            original.GetData(pixels);
            texture.SetData(0, null, pixels, 0, pixels.Length);

            // 4. MipMap-Kette erstellen
            Texture2D currentLevel = original;
            for (int level = 1; level < mipLevels; level++)
            {
                width = int.Max(width / 2, 1);
                height = int.Max(height / 2, 1);

                // Pixel für nächstes Level generieren (Box-Downscale)
                Texture2D nextLevelTexture = Texture2DScaler.DownscaleBox(GraphicsDevice, currentLevel);
                Color[] mipPixels = new Color[width * height];
                nextLevelTexture.GetData(mipPixels);

                // In die Texture schreiben
                texture.SetData(level, null, mipPixels, 0, mipPixels.Length);

                currentLevel = nextLevelTexture;
            }

            return texture;
        }

        public override void ApplyDefinition()
        {
            base.ApplyDefinition();

            _filterMode = Definition.FilterMode;
            _addressMode = Definition.AddressMode;
            _translation = Definition.Translation;
            _rotation = Definition.Rotation;
            _scale = Definition.Scale;
            _pivot = Definition.Pivot;
        }

        public override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);

            switch (key)
            {
                case nameof(Texture2DDefinition.FilterMode):
                    _filterMode = Definition.FilterMode;
                    break;
                case nameof(Texture2DDefinition.AddressMode):
                    _addressMode = Definition.AddressMode;
                    break;
                case nameof(Texture2DDefinition.Translation):
                    _translation = Definition.Translation;
                    _transformDirty = true;
                    break;
                case nameof(Texture2DDefinition.Rotation):
                    _rotation = Definition.Rotation;
                    _transformDirty = true;
                    break;
                case nameof(Texture2DDefinition.Scale):
                    _scale = Definition.Scale;
                    _transformDirty = true;
                    break;
                case nameof(Texture2DDefinition.Pivot):
                    _pivot = Definition.Pivot;
                    _transformDirty = true;
                    break;
            }
        }
    }
}
