using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Components.Rendering;
using System;

namespace Sachssoft.Sasogine.Rendering.Parallax
{
    public class ScrollParallaxLayerDefinition : IScrollParallaxLayerDefinition
    {
        public Vector2 ScrollSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector2 Spacing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector2 Factor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Index { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Depth { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ComponentReference<Texture2DAsset> Texture { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector2 Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector2 Offset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public StretchMode StretchMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Alignment VerticalAlignment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Alignment HorizontalAlignment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<DefinitionChangedEventArgs>? Changed;
    }
}
