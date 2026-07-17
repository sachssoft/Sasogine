using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class EffectAsset : AssetBase<IShader, EffectAssetDefinition>
    {
        public EffectAsset() : base(new EffectAssetDefinition()) { }

        public EffectAsset(EffectAssetDefinition definition) : base(definition) { }

        public GraphicsDevice? GraphicsDevice { get; set; }

        protected override EffectAssetDefinition ResolveDefinition()
        {
            return new EffectAssetDefinition();
        }

        protected override IShader? Build(Stream stream)
        {
            if (GraphicsDevice == null)
                throw new InvalidOperationException($"{nameof(Texture2DAsset)} requires a valid {nameof(GraphicsDevice)} before calling {nameof(Build)}.");

            // Lies den Stream komplett in ein Byte-Array
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] effectBytes = ms.ToArray();

            if (Definition.Template == null)
                return null;

            var effect = Definition.Template.Create();
            effect.GraphicsDevice = GraphicsDevice;

            // Effekt direkt aus Byte-Array erzeugen
            return effect;
        }

    }
}
