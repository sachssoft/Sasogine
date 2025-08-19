/* 
 * © 2024 Tobias Sachs
 * ImportedEffect
 * 16.07.2024 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics;

public abstract class ImportedEffect : IEffect
{
    private Effect _effect;

    public ImportedEffect(Effect effect)
    {
        _effect = effect;
    }

    public Effect Effect => _effect;

    public virtual Matrix Projection
    {
        get;
        set;
    } = Matrix.Identity;

    public virtual Matrix View
    {
        get;
        set;
    } = Matrix.Identity;

    public virtual Matrix World
    {
        get;
        set;
    } = Matrix.Identity;

    public virtual Texture2D? Texture
    {
        get;
        set;
    } = null;

    public virtual Color Color
    {
        get;
        set;
    } = Color.White;

    public virtual float Opacity
    {
        get;
        set;
    } = 1f;

    public EffectTechnique CurrentTechnique
    {
        get => _effect.CurrentTechnique;
        set => _effect.CurrentTechnique = value;
    }

    Effect IEffect.Result => _effect;

    public abstract ImportedEffect Clone();

    object ICloneable.Clone() => Clone();

    public virtual void Apply()
    {
    }

    public void Dispose()
    {
        _effect.Dispose();
    }
}
