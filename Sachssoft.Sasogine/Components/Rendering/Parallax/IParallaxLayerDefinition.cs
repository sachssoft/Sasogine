using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;

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
