/* 
 * © 2024 Tobias Sachs
 * ImplementedBasicEffect
 * 09.07.2024 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Graphics;

public class ImplementedBasicEffect : BasicEffect, IEffect
{
    public ImplementedBasicEffect() : base(IMyGameApp.Current.GraphicsDevice)
    {
    }

    public ImplementedBasicEffect(GraphicsDevice device) : base(device)
    {
        VertexColorEnabled = true;
        TextureEnabled = true;
        Opacity = 1f;
    }

    public Color Color
    {
        get => new Color(DiffuseColor);
        set => DiffuseColor = value.ToVector3();
    }

    public float Opacity
    {
        get => Alpha;
        set => Alpha = value;
    }

    Effect IEffect.Result => this;

    void IEffect.Apply()
    {        
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
