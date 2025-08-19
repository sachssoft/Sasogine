using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Graphics.Renderer;

public sealed class ModelRenderer : RendererBase
{
    private readonly Model _model;
    private CameraBase? _camera;

    public ModelRenderer(CameraBase camera, IEffect effect, Model model)
        : base(IMyGameApp.Current.GraphicsDevice, effect, camera)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    protected override void OnInitialize(object[] args)
    {
        _camera = (CameraBase)args[0];

        BlendState = BlendState.Opaque;
        SamplerState = SamplerState.LinearWrap;
        DepthStencil = DepthStencilState.Default;

        Rasterizer.CullMode = CullMode.CullCounterClockwiseFace;
        Rasterizer.MultiSampleAntiAlias = true;

        Effect.Projection = _camera.Projection;
        Effect.View = _camera.View;
        Effect.World = _camera.World;
    }

    protected override void OnUninitialize()
    {
        // Optional: Cleanup
    }

    public CameraBase Camera => _camera!;

    public void DrawModel(Matrix? world_transform = null, Action<BasicEffect>? configure_effect = null)
    {
        Matrix world = (world_transform ?? Matrix.Identity) * _camera!.World;

        foreach (ModelMesh mesh in _model.Meshes)
        {
            foreach (Effect base_effect in mesh.Effects)
            {
                if (base_effect is BasicEffect effect)
                {
                    effect.World = world;
                    effect.View = _camera.View;
                    effect.Projection = _camera.Projection;
                    effect.EnableDefaultLighting();
                    configure_effect?.Invoke(effect);
                }
            }

            mesh.Draw();
        }
    }
}
