/* 
 * © 2024 Tobias Sachs
 * CameraBase
 * 11.07.2024 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics;

public abstract class CameraBase
{
    private Matrix _world = Matrix.Identity;
    private GraphicsDevice _graphic_device;
    private Matrix _projection;
    private Matrix _view;

    public CameraBase(GraphicsDevice graphic_device)
    {
        _graphic_device = graphic_device;
    }

    public GraphicsDevice GraphicsDevice => _graphic_device;

    public Matrix Projection
    {
        get => _projection;
        protected set => _projection = value;
    }

    public Matrix View
    {
        get => _view;
        protected set => _view = value;
    }

    public Matrix World
    {
        get => _world;
        protected set => _world = value;
    }

    //public Matrix Transform => World * View * Projection;

    public void ApplyEffect(IEffectAdapter effect)
    {
        effect.Projection = _projection;
        effect.View = _view;
        effect.World = _world;
    }

    public virtual Vector2 ToWorld(Vector2 screen_position)
    {
        var unprojected = _graphic_device.Viewport.Unproject(new Vector3(screen_position, 0f), _projection, _view, _world);
        return new Vector2(unprojected.X, unprojected.Y);
    }

    public virtual Vector2 ToScreen(Vector2 world_position)
    {
        var projected = _graphic_device.Viewport.Project(new Vector3(world_position, 0f), _projection, _view, _world);
        return new Vector2(projected.X, projected.Y);
    }

    public virtual void Update(GameFrameContext context)
    {
    }

    public virtual bool IsInView(Vector3 min, Vector3 max, Matrix? world = null)
    {
        var worldMatrix = world ?? Matrix.Identity;

        var box = new BoundingBox(min, max);

        // BoundingBox transformieren, falls nötig
        if (worldMatrix != Matrix.Identity)
            box = TransformBoundingBox(box, worldMatrix);

        var frustum = new BoundingFrustum(World * View * Projection);
        return frustum.Intersects(box);
    }

    private static BoundingBox TransformBoundingBox(BoundingBox box, Matrix matrix)
    {
        Vector3[] corners = box.GetCorners();
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = Vector3.Transform(corners[i], matrix);
        }
        return BoundingBox.CreateFromPoints(corners);
    }


}
