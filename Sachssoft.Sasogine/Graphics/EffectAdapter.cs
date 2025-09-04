using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;

public abstract class EffectAdapter : IEffectAdapter
{
    private Effect _effect;

    public EffectAdapter(Effect effect)
    {
        _effect = effect;
    }

    public Effect InnerEffect => _effect;

    public virtual Matrix Projection { get; set; } = Matrix.Identity;
    public virtual Matrix View { get; set; } = Matrix.Identity;
    public virtual Matrix World { get; set; } = Matrix.Identity;

    public virtual Texture2D? Texture { get; set; } = null;
    public virtual Color Color { get; set; } = Color.White;
    public virtual float Opacity { get; set; } = 1f;

    public EffectTechnique CurrentTechnique
    {
        get => _effect.CurrentTechnique;
        set => _effect.CurrentTechnique = value;
    }

    public abstract EffectAdapter Clone();
    object ICloneable.Clone() => Clone();

    public virtual void Apply()
    {
    }

    public void Dispose()
    {
        _effect.Dispose();
    }
}
