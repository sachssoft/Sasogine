using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Cameras;

namespace Sachssoft.Sasogine.ShapeBatchTests;

/// <summary>Provides an identity camera for a fixed offscreen test target.</summary>
internal sealed class TestCamera : ICamera
{
    public Matrix Projection => Matrix.Identity;
    public Matrix View => Matrix.Identity;
    public Matrix World => Matrix.Identity;
    public void ApplyViewport(Viewport viewport) { }
    public Point2 ToWorld(Point2 point) => point;
    public Point2 ToScreen(Point2 point) => point;
    public void Update(GameContext context) { }
    public void Reset() { }
    public ICamera Clone() => new TestCamera();
    object ICloneable.Clone() => Clone();
}
