using Microsoft.Xna.Framework;
using Sachssoft.Observables;
using Sachssoft.Sasogine.Elements;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;

namespace Sachssoft.Sasogine.Graphics;

public class RenderTransform : NotifyingObject, ITransform
{
    private Vector2 _translation = Vector2.Zero;
    private float _rotation = 0f;
    private Vector2 _scale = Vector2.One;
    private Vector2 _origin = new Vector2(0.5f);

    public Vector2 Translation
    {
        get => _translation;
        set => this.RaiseAndSetIfChanged(ref _translation, value);
    }

    public float Rotation
    {
        get => _rotation;
        set => this.RaiseAndSetIfChanged(ref _rotation, value);
    }

    public float RotationDegree
    {
        get => MathHelper.ToDegrees(_rotation);
        set => _rotation = MathHelper.ToRadians(value);
    }

    public Vector2 Scale
    {
        get => _scale;
        set => this.RaiseAndSetIfChanged(ref _scale, value);
    }

    public Vector2 Origin
    {
        get => _origin;
        set => this.RaiseAndSetIfChanged(ref _origin, value);
    }

    public Matrix ToMatrix()
    {
        return MatrixHelper.Create(Translation, new Vector2(1f / Scale.X, 1f / Scale.Y), Rotation, Origin);
    }
}
