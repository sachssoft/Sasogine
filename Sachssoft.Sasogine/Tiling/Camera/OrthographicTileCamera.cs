using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine.Tiling;

public class OrthographicTileCamera : FollowableCamera2D
{

    public OrthographicTileCamera() : this(IMyGameApp.Current.GraphicsDevice)
    {
    }

    public OrthographicTileCamera(GraphicsDevice device) : base(device)
    {
        ZoomRatioUsed = true;
        ZoomFactor = 5f;
        ZoomMinimum = 0.2f;
        ZoomMaximum = 2f;
        PlaneMinimum = -50f;
        PlaneMaximum = 50f;
    }

    protected override Matrix ViewOverride()
    {
        var position = new Vector3(Position, 0f);
        var camera_up = Vector3.TransformNormal(Vector3.Up, Matrix.CreateRotationZ(0f));

        return Matrix.CreateLookAt(position, position + Vector3.Forward, camera_up);
    }

    public Vector2 GetCameraSize(Point screen_size)
    {
        var f = GetEffectiveZoomFactor();
        return new(screen_size.X * f, screen_size.Y * f);
    }

    public override Vector2 ToWorld(Vector2 screen_position)
    {
        var t = new Vector3(screen_position, 0);
        t = IMyGameApp.Current.GraphicsDevice.Viewport.Unproject(t, Projection, View, World);
        return new Vector2(t.X, t.Y);
    }

    public override Vector2 ToScreen(Vector2 world_position)
    {
        var t = new Vector3(world_position, 0);
        t = IMyGameApp.Current.GraphicsDevice.Viewport.Project(t, Projection, View, World);
        return new Vector2(t.X, t.Y);
    }

}
