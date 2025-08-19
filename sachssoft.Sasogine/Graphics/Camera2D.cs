using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Animation;
using Sachssoft.Sasogine.Animation.Timings;
using System;
using System.Net.Http.Headers;

namespace Sachssoft.Sasogine.Graphics;

public abstract class Camera2D : CameraBase
{
    private Vector2 _move_start;
    private Vector2 _move_end;
    private float _move_time;
    private float _move_duration;
    private AnimationTimingBase? _move_timing;
    private bool _move_animating;

    private Vector2 _position = Vector2.Zero;
    private Vector2 _origin = Vector2.Zero;
    private float _zoom = 1f;

    private float _zoom_start;
    private float _zoom_end;
    private float _zoom_time;
    private float _zoom_duration;
    private AnimationTimingBase? _zoom_timing;
    private bool _zoom_animating;

    private float _plane_minimum = -10f;
    private float _plane_maximum = 10f;

    private bool _zoom_ratio_used = true;
    private float _zoom_factor = 1.3f;

    // Einfache Min/Max Zoomwerte
    private float _zoom_minimum = 0.01f;
    private float _zoom_maximum = 10f;

    private Vector2 _position_minimum = new Vector2(float.MinValue, float.MinValue);
    private Vector2 _position_maximum = new Vector2(float.MaxValue, float.MaxValue);
    private float _zoom_animation_duration = 0.5f;

    protected Camera2D(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        // UpdateZoomBounds entfernt - keine Exponenten mehr
    }

    public bool IsMoveAnimating => _move_animating;


    public Vector2 Position
    {
        get => _position;
        set => _position = Vector2.Clamp(value, _position_minimum, _position_maximum);
    }

    public Vector2 Origin
    {
        get => _origin;
        set => _origin = value;
    }

    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = MathHelper.Clamp(value, _zoom_minimum, _zoom_maximum);
        }
    }

    public Vector2 PositionMinimum
    {
        get => _position_minimum;
        set
        {
            _position_minimum = value;
            ClampPosition();
        }
    }

    public Vector2 PositionMaximum
    {
        get => _position_maximum;
        set
        {
            _position_maximum = value;
            ClampPosition();
        }
    }

    /// <summary>
    /// Startet eine animierte Bewegung der Kamera von aktueller Position zu target_position.
    /// </summary>
    /// <param name="target_position">Zielposition</param>
    /// <param name="duration">Dauer in Sekunden</param>
    /// <param name="timing">Timingfunktion für sanfte Bewegung</param>
    public void MoveTo(Vector2 target_position, float duration, AnimationTimingBase? timing = null)
    {
        _move_start = _position;
        _move_end = Vector2.Clamp(target_position, _position_minimum, _position_maximum);
        _move_duration = MathF.Max(duration, 0.001f); // Vermeide Division durch 0
        _move_time = 0f;
        _move_timing = timing ?? new EaseOutQuad();
        _move_animating = true;
    }

    public float ZoomMinimum
    {
        get => _zoom_minimum;
        set
        {
            _zoom_minimum = value;
            ClampZoom();
        }
    }

    public float ZoomMaximum
    {
        get => _zoom_maximum;
        set
        {
            _zoom_maximum = value;
            ClampZoom();
        }
    }

    public float ZoomAnimationSpeed
    {
        get => 1f / _zoom_animation_duration;
        set
        {
            _zoom_animation_duration = 1f / value;
            ClampZoom();
        }
    }

    public float PlaneMinimum
    {
        get => _plane_minimum;
        protected set => _plane_minimum = MathF.Min(value, 0f);
    }

    public float PlaneMaximum
    {
        get => _plane_maximum;
        protected set => _plane_maximum = MathF.Max(value, 0f);
    }

    public bool ZoomRatioUsed
    {
        get => _zoom_ratio_used;
        protected set => _zoom_ratio_used = value;
    }

    public float ZoomFactor
    {
        get => _zoom_factor;
        set
        {
            _zoom_factor = value;
            StopZoomAnimation();
        }
    }

    public void Reset()
    {
        _position = Vector2.Zero;
        _origin = Vector2.Zero;
        _zoom = 1f;
        StopZoomAnimation();
        _move_animating = false;
    }

    public float GetEffectiveZoomFactor()
    {
        float factor = 1f / _zoom_factor;

        if (_zoom_ratio_used)
        {
            float screen_width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            float screen_height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            if (screen_height != 0f)
            {
                float ratio = screen_width / screen_height;
                return factor * ratio;
            }
        }

        return factor;
    }

    protected virtual Matrix ProjectionOverride()
    {
        var factor = GetEffectiveZoomFactor();

        float width = GraphicsDevice.PresentationParameters.BackBufferWidth * factor * _zoom;
        float height = GraphicsDevice.PresentationParameters.BackBufferHeight * factor * _zoom;

        return Matrix.CreateOrthographic(width * factor, height * factor, _plane_minimum, _plane_maximum);
    }

    protected virtual Matrix ViewOverride()
    {
        return Matrix.Identity;
    }

    protected virtual Matrix WorldOverride()
    {
        return Matrix.Identity;
    }

    public override void Update(GameContext context)
    {
        float dt = (float)context.GameTime.ElapsedGameTime.TotalSeconds;

        if (_zoom_animating && _zoom_timing is not null)
        {
            _zoom_time += dt;

            float percent = MathF.Min(_zoom_time / _zoom_duration, 1f);
            float t = _zoom_timing.GetValue(percent);

            float new_zoom = LogLerp(_zoom_start, _zoom_end, t);

            _zoom = new_zoom;

            if (percent >= 1f)
            {
                _zoom_animating = false;
                _zoom = _zoom_end;
            }

            ClampZoom();
        }

        if (_move_animating && _move_timing is not null)
        {
            _move_time += dt;
            float percent = MathF.Min(_move_time / _move_duration, 1f);
            float t = _move_timing.GetValue(percent);

            Position = Vector2.Lerp(_move_start, _move_end, t);

            if (percent >= 1f)
            {
                _move_animating = false;
                Position = _move_end;
            }
        }

        Projection = ProjectionOverride();
        View = ViewOverride();
        World = WorldOverride();
    }

    /// <summary>
    /// Logarithmische Interpolation zwischen start und end basierend auf t (0..1)
    /// </summary>
    private float LogLerp(float start, float end, float t)
    {
        start = MathF.Max(start, 0.0001f);
        end = MathF.Max(end, 0.0001f);

        float logStart = MathF.Log(start);
        float logEnd = MathF.Log(end);

        float logResult = MathHelper.Lerp(logStart, logEnd, t);
        return MathF.Exp(logResult);
    }

    public void Move(Vector2 amount)
    {
        Position += amount;  // intuitiver: Bewege um amount nach rechts/oben
    }

    public void ZoomIn(AnimationTimingBase? timing = null)
    {
        timing ??= new EaseOutQuad();
        float target_zoom = _zoom / _zoom_factor * 10f;
        float duration = MathF.Abs(MathF.Log(_zoom) - MathF.Log(target_zoom)) * _zoom_animation_duration;
        ZoomTo(target_zoom, duration, timing);
    }

    public void ZoomOut(AnimationTimingBase? timing = null)
    {
        timing ??= new EaseOutQuad();
        float target_zoom = _zoom * _zoom_factor / 10f;
        float duration = MathF.Abs(MathF.Log(_zoom) - MathF.Log(target_zoom)) * _zoom_animation_duration;
        ZoomTo(target_zoom, duration, timing);
    }

    /// <summary>
    /// Setzt den Zoomfaktor der Kamera auf den Standardwert (z.B. 1.0f) zurück.
    /// </summary>
    public void ResetZoom()
    {
        Zoom = 1.0f;
    }

    public void ZoomToWithFocus(float target_zoom, Vector2 focus_world_position, float duration, AnimationTimingBase? timing = null)
    {
        // Aktuelles Verhältnis berechnen
        float current_zoom = _zoom;
        float clamped_target_zoom = MathHelper.Clamp(target_zoom, _zoom_minimum, _zoom_maximum);

        // Bildschirmmittelpunkt ermitteln
        Vector2 screen_center = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f;

        // Bildschirmmittelpunkt in Weltkoordinaten (vorher)
        Vector2 world_center_before = ToWorld(screen_center);

        // Zielposition berechnen, damit focus_world_position im Bildschirmzentrum bleibt
        float scale_factor = clamped_target_zoom / current_zoom;
        Vector2 offset = (focus_world_position - world_center_before) * scale_factor;

        // Neue Kameraposition berechnen
        Vector2 new_camera_position = _position + offset;
        _position = Vector2.Clamp(new_camera_position, _position_minimum, _position_maximum);

        // Zoom starten
        ZoomTo(target_zoom, duration, timing ?? new EaseOutQuad());
    }

    /// <summary>
    /// Zoomt zu target_zoom und hält dabei den Weltpunkt unter screen_position an gleicher Stelle.
    /// </summary>
    public void ZoomTo(float target_zoom, float duration, AnimationTimingBase? timing = null)
    {
        _zoom_start = _zoom;
        _zoom_end = MathHelper.Clamp(target_zoom, _zoom_minimum, _zoom_maximum);
        _zoom_duration = MathF.Max(duration, 0.001f);
        _zoom_time = 0f;
        _zoom_timing = timing ?? new EaseOutQuad();
        _zoom_animating = true;
    }

    public void StopZoomAnimation()
    {
        _zoom_animating = false;
    }

    private void ClampPosition()
    {
        _position = Vector2.Clamp(_position, _position_minimum, _position_maximum);
    }

    private void ClampZoom()
    {
        _zoom = MathHelper.Clamp(_zoom, _zoom_minimum, _zoom_maximum);
    }

    public override Vector2 ToWorld(Vector2 screenPosition)
    {
        Matrix inv = Matrix.Invert(View * Projection);
        Vector3 screenPos3 = new Vector3(screenPosition, 0f);
        Vector3 worldPos3 = Vector3.Transform(screenPos3, inv);
        return new Vector2(worldPos3.X, worldPos3.Y);
    }

    public override Vector2 ToScreen(Vector2 worldPosition)
    {
        Vector3 worldPos3 = new Vector3(worldPosition, 0f);
        Vector3 screenPos3 = Vector3.Transform(worldPos3, View * Projection);
        return new Vector2(screenPos3.X, screenPos3.Y);
    }

    /// <summary>
    /// Liefert die BoundingBox in Weltkoordinaten, die aktuell vom Bildschirm sichtbar ist.
    /// </summary>
    public BoundingBox GetScreenBoundingBox()
    {
        var viewport = GraphicsDevice.Viewport;

        Vector2 topLeft = ToWorld(Vector2.Zero);
        Vector2 topRight = ToWorld(new Vector2(viewport.Width, 0));
        Vector2 bottomLeft = ToWorld(new Vector2(0, viewport.Height));
        Vector2 bottomRight = ToWorld(new Vector2(viewport.Width, viewport.Height));

        float minX = MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X));
        float maxX = MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomLeft.X, bottomRight.X));
        float minY = MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomLeft.Y, bottomRight.Y));
        float maxY = MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomLeft.Y, bottomRight.Y));

        return new BoundingBox(new Vector3(minX, minY, 0f), new Vector3(maxX, maxY, 0f));
        //return new BoundingBox(minX, minY, width, height);
    }
}
