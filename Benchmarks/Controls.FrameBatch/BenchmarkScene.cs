using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Components.Rendering;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Provides the minimal scene required by the shared Sasogine host.</summary>
internal sealed class BenchmarkScene : BasicSceneBase
{
    /// <inheritdoc/>
    public override ICamera CreateCamera(GraphicsDevice graphicsDevice, int index) => new Camera2();
}
