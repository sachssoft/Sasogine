using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    public class ParallaxLayerDefinitionBase : IComponentDefinition
    {
        public string? Id { get; set; }

        public string? Class { get; set; }

        // Reihenfolge des Layers
        public int Index { get; set; }

        // Name des Layers
        public string Name { get; set; }

        public float Depth { get; set; }

        public Reference<Texture2DAsset>? Texture { get; set; }

        public Vector2 Scale { get; set; }

        public Vector2 Offset { get; set; }

        public StretchMode StretchMode { get; set; }

        public Alignment VerticalAlignment { get; set; }

        public Alignment HorizontalAlignment { get; set; }
    }
}
