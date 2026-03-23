using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    [Obsolete("ParallaxEntry is deprecated. Use ParallaxLayer instead.")]
    public record ParallaxEntry
    {
        private Texture2D? _texture;

        public ParallaxEntry()
        {
        }

        public Texture2D? Texture
        {
            get => _texture;
            init => _texture = value?.CreatePremultiplied();
        }

        public int LayerIndex { get; init; }

        public string? Layer { get; init; }

        public float Depth { get; init; }

        public Vector2 TextureScale { get; init; } = Vector2.One;

        public Vector2 Spacing { get; init; }

        public Vector2 ScrollSpeed { get; init; }

        public Vector2 Offset { get; init; }

        public StretchMode StretchMode { get; init; }

        public RepeatMode RepeatMode { get; init; }

        public Alignment VerticalAlignment { get; init; }

        public Alignment HorizontalAlignment { get; init; }

        public ScrollingBehavior ScrollingBehavior { get; init; }


    }
}
