/* 
 * © 2024 Tobias Sachs
 * IEffect
 * 09.07.2024 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace sachssoft.Sasogine.Graphics;

public interface IEffect : IDisposable, ICloneable, ITransformProvider
{
    Texture2D? Texture { get; set; }

    EffectTechnique CurrentTechnique { get; set; }

    Color Color { get; set; }

    float Opacity { get; set; }

    void Apply();

    Effect Result { get; }

}
