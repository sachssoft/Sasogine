using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Three-dimensional transform data applied to the logo plane before projection.</summary>
internal sealed class LogoSpatialDefinition : Element, IDefinition
{
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale = Vector3.One;
    private static readonly PropertyChangedEventArgs PositionArgs = new(nameof(Position));
    private static readonly PropertyChangedEventArgs RotationArgs = new(nameof(Rotation));
    private static readonly PropertyChangedEventArgs ScaleArgs = new(nameof(Scale));
    /// <summary>Gets or sets the spatial offset.</summary>
    public Vector3 Position { get => _position; set => SetNotify(ref _position, Vector3.Clamp(value, new(-180), new(180)), PositionArgs); }
    /// <summary>Gets or sets Euler rotation angles in degrees.</summary>
    public Vector3 Rotation { get => _rotation; set => SetNotify(ref _rotation, Vector3.Clamp(value, new(-180), new(180)), RotationArgs); }
    /// <summary>Gets or sets spatial scale.</summary>
    public Vector3 Scale { get => _scale; set => SetNotify(ref _scale, Vector3.Clamp(value, new(.5f), new(1.4f)), ScaleArgs); }
}
