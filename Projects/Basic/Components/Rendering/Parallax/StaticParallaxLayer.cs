using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Provides a static parallax layer that does not produce an additional runtime displacement.
/// </summary>
public class StaticParallaxLayer : ParallaxLayerBase
{
    /// <inheritdoc/>
    public override Vector2 ParallaxOffset => Vector2.Zero;

    /// <inheritdoc/>
    public override void Update(SceneUpdateContext context)
    {
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!IsVisible)
            return;
    }
}
