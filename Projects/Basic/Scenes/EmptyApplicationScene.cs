using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Cameras;
using System;

namespace Sachssoft.Engine.Scenes;

internal sealed class EmptyApplicationScene : BasicSceneBase, IScene
{
    bool IScene.RequiresCamera => false;

    /// <inheritdoc/>
    public override ICamera CreateCamera(GraphicsDevice graphicsDevice, int index) =>
        throw new InvalidOperationException("The empty application scene does not use a camera.");
}
