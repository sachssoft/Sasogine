using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Materials;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Graphics.Primitives.Sprites
{
    public sealed class SpritePrimitive : IPrimitive
    {
        public bool IsVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Matrix Transform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Rectangle SourceRectangle { get; set; }

        public ICamera? CustomCamera { get; set; }

        //public IShader CustomShader { get; set; }

        public IMaterial Material { get; set; }

        public void Draw(SceneDrawContext context)
        {

            Material.Apply();

        }
    }
}
