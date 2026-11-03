using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Sasogine.ShapeBatchTests;

/// <summary>Uses MonoGame's built-in effect to validate actual ShapeBatch vertex/index uploads and blending.</summary>
internal sealed class TestShader : IShader
{
    private readonly BasicEffect _effect;

    internal TestShader(GraphicsDevice device)
    {
        GraphicsDevice = device;
        _effect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            TextureEnabled = true,
            World = Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.CreateOrthographicOffCenter(0, 512, 512, 0, 0, 1)
        };
    }

    public GraphicsDevice GraphicsDevice { get; set; }
    public EffectTechnique CurrentTechnique { get => _effect.CurrentTechnique; set => _effect.CurrentTechnique = value; }
    public Texture2D? Texture { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Opacity { get; set; } = 1;
    public Effect Effect => _effect;
    public void Apply() { _effect.DiffuseColor = Color.ToVector3(); _effect.Alpha = Opacity * Color.A / 255f; }
    public object Clone() => new TestShader(GraphicsDevice) { Color = Color, Opacity = Opacity };
    public void Dispose() => _effect.Dispose();
}
