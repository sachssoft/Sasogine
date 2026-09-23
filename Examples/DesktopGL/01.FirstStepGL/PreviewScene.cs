using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Components.Rendering;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Graphics.Meshes;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Examples;

class PreviewScene : BasicSceneBase
{
    private GraphicsDevice? _graphicsDevice;
    private IMesh? _mesh;
    private float _rotation;

    public PreviewScene()
    {
        BackgroundColor = Color.DarkBlue;
    }

    public override ICamera CreateCamera(GraphicsDevice graphicsDevice, int index)
    {
        return new Camera2();
    }

    public override void Enter(SceneEnterEventArgs args)
    {
        base.Enter(args);

        _graphicsDevice = args.Application.GraphicsDevice;
    }

    public override void Load()
    {
        base.Load();

        VertexPositionColor[] vertices =
        [
            new(new Vector3(0.0f, 1.0f, 0.0f), Color.Red),
            new(new Vector3(-1.0f, -1.0f, 0.0f), Color.Green),
            new(new Vector3(1.0f, -1.0f, 0.0f), Color.Blue)
        ];

        short[] indices = [0, 1, 2];

        _mesh = new Mesh<VertexPositionColor>(
            _graphicsDevice!,
            vertices,
            indices);
    }

    public override void Update(SceneUpdateContext context)
    {
        base.Update(context);

        _rotation += MathHelper.PiOver2 * context.ElapsedTimeInSeconds;
    }

    public override void Draw(SceneDrawContext context)
    {
        base.Draw(context);

        if (_mesh is null)
            return;

        context.DefaultMaterial.Shader.Opacity = 1f;
        context.DefaultMaterial.Shader.Color = Color.White;
        context.DefaultMaterial.Apply();

        using var scope = new RenderScope(
            context.GraphicsDevice,
            new RenderOptions
            {
                CullMode = CullMode.None,
                Depth = DepthMode.Disabled,
                AlphaBlend = false
            });

        var transform =
            Matrix.CreateScale(0.5f) *
            Matrix.CreateRotationZ(_rotation);

        MeshRenderer.Draw(context, _mesh, transform: transform);
    }
}