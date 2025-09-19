using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Kamera, die einem Zielpunkt oder -objekt folgen kann, mit optionaler Dämpfung.
/// </summary>
public class FollowableCamera2D : Camera2D
{
    private Vector2 _follow_target;
    private bool _follow_enabled = false;

    private float _follow_damping = 0.1f;
    private bool _use_damping = true;

    private BoundingBox? _dead_zone = null;

    /// <summary>
    /// Erstellt eine neue Follow-Kamera.
    /// </summary>
    public FollowableCamera2D(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
    }

    /// <summary>
    /// Aktiviert oder deaktiviert das Folgen.
    /// </summary>
    public bool FollowEnabled
    {
        get => _follow_enabled;
        set => _follow_enabled = value;
    }

    /// <summary>
    /// Zielpunkt, dem gefolgt werden soll (in Weltkoordinaten).
    /// </summary>
    public Vector2 FollowTarget
    {
        get => _follow_target;
        set => _follow_target = value;
    }

    /// <summary>
    /// Gibt an, ob gedämpftes Folgen aktiv ist.
    /// </summary>
    public bool UseDamping
    {
        get => _use_damping;
        set => _use_damping = value;
    }

    /// <summary>
    /// Dämpfungsfaktor (je kleiner, desto langsamer folgt die Kamera).
    /// </summary>
    public float FollowDamping
    {
        get => _follow_damping;
        set => _follow_damping = MathHelper.Clamp(value, 0.001f, 1f);
    }

    /// <summary>
    /// Optionaler Bereich im Bildschirmzentrum, in dem sich das Ziel bewegen darf, ohne dass die Kamera folgt.
    /// </summary>
    public BoundingBox? DeadZone
    {
        get => _dead_zone;
        set => _dead_zone = value;
    }

    public override void Update(GameFrameContext context)
    {
        base.Update(context); // Verarbeitet Animations-Updates

        if (!_follow_enabled || IsMoveAnimating)
            return;

        Vector2 current_screen_center = ToScreen(Position);
        Vector2 target_screen_position = ToScreen(_follow_target);

        bool outsideDeadZone = true;

        if (_dead_zone is BoundingBox dz)
        {
            var point = new Vector3(target_screen_position, 0f);
            bool inside = dz.Contains(point) == ContainmentType.Contains;
            outsideDeadZone = !inside;
        }

        if (outsideDeadZone)
        {
            Vector2 delta = _follow_target - Position;

            if (_use_damping)
            {
                Position += delta * _follow_damping;
            }
            else
            {
                Position = _follow_target;
            }
        }
    }
}
