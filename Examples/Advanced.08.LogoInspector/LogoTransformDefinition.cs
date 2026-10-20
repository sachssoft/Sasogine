using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Observable configuration for the logo example; no member discovery is used.</summary>
internal sealed class LogoTransformDefinition : Element, IDefinition
{
    private Vector2 _Position;
    private static readonly PropertyChangedEventArgs PositionArgs = new(nameof(Position));
    /// <summary>Gets or sets the planar offset as one atomic vector property.</summary>
    public Vector2 Position { get => _Position; set => SetNotify(ref _Position, Vector2.Clamp(value, new(-180, -120), new(180, 120)), PositionArgs); }

    private Vector2 _Scale = Vector2.One;
    private static readonly PropertyChangedEventArgs ScaleArgs = new(nameof(Scale));
    /// <summary>Gets or sets scale.</summary>
    public Vector2 Scale { get => _Scale; set => SetNotify(ref _Scale, Vector2.Clamp(value, new(.5f), new(1.4f)), ScaleArgs); }

    private float _Rotation = 0;
    private static readonly PropertyChangedEventArgs RotationArgs = new(nameof(Rotation));
    /// <summary>Gets or sets rotation.</summary>
    public float Rotation { get => _Rotation; set => SetNotify(ref _Rotation, Math.Clamp(value, -180, 180), RotationArgs); }
}
