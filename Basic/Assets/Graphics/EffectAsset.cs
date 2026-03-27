using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class EffectAsset : AssetBase<IEffectAdapter, IEffectDefinition>
    {
        public GraphicsDevice? GraphicsDevice { get; set; }

        protected override IEffectAdapter? Build(Stream stream)
        {
            if (GraphicsDevice == null)
                throw new InvalidOperationException($"{nameof(Texture2DAsset)} requires a valid {nameof(GraphicsDevice)} before calling {nameof(Build)}.");

            // Lies den Stream komplett in ein Byte-Array
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] effectBytes = ms.ToArray();

            var effect = Definition.Template.Create();
            effect.GraphicsDevice = GraphicsDevice;

            // Effekt direkt aus Byte-Array erzeugen
            return effect;
        }

    }
}
