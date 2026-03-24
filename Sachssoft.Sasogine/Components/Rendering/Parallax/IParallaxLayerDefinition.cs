using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Components.Rendering
{
    public interface IParallaxLayerDefinition : IComponentDefinition
    {
        // Reihenfolge des Layers
        int Index { get; set; }

        // Name des Layers
        string Name { get; set; }

        float Depth{ get; set; }

        AssetReference<Texture2DAsset> Texture { get; set; }

        Vector2 Scale { get; set; }

        Vector2 Offset { get; set; }

        StretchMode StretchMode { get; set; }

        Alignment VerticalAlignment { get; set; }

        Alignment HorizontalAlignment { get; set; }
    }
}
