using System;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Tracks the state of a two-dimensional transform source and provides
/// information about transform values that have changed.
/// </summary>
/// <remarks>
/// <para>
/// The available transform capabilities are determined once when the state
/// is created and are cached for subsequent change checks.
/// </para>
/// <para>
/// Calling <see cref="UpdateState"/> updates the stored state to match the
/// current values of the transform source.
/// </para>
/// </remarks>
public sealed class Transform2State
{
    private readonly IReadOnlyTransformPosition2? _positionSource;
    private readonly IReadOnlyTransformSize2? _sizeSource;
    private readonly IReadOnlyTransformScale2? _scaleSource;
    private readonly IReadOnlyTransformRotation2? _rotationSource;
    private readonly IReadOnlyTransformRotationPivot2? _rotationPivotSource;
    private readonly IReadOnlyTransformSkew2? _skewSource;

    private Point2 _position;
    private Size2 _size;
    private Vector2 _scale;
    private float _rotation;
    private Point2 _rotationPivot;
    private Vector2 _skew;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transform2State"/> class.
    /// </summary>
    /// <param name="source">
    /// The transform source whose state is tracked.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public Transform2State(ITransform2 source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _positionSource = source as IReadOnlyTransformPosition2;
        _sizeSource = source as IReadOnlyTransformSize2;
        _scaleSource = source as IReadOnlyTransformScale2;
        _rotationSource = source as IReadOnlyTransformRotation2;
        _rotationPivotSource = source as IReadOnlyTransformRotationPivot2;
        _skewSource = source as IReadOnlyTransformSkew2;

        UpdateState();
    }

    /// <summary>Gets the tracked position.</summary>
    public Point2 Position => _position;

    /// <summary>Gets the tracked size.</summary>
    public Size2 Size => _size;

    /// <summary>Gets the tracked scale.</summary>
    public Vector2 Scale => _scale;

    /// <summary>Gets the tracked rotation.</summary>
    public float Rotation => _rotation;

    /// <summary>Gets the tracked rotation pivot.</summary>
    public Point2 RotationPivot => _rotationPivot;

    /// <summary>Gets the tracked skew.</summary>
    public Vector2 Skew => _skew;

    /// <summary>
    /// Gets a value indicating whether any tracked transform value has changed.
    /// </summary>
    public bool IsChanged
    {
        get
        {
            if (_positionSource != null && _positionSource.Position != _position)
                return true;

            if (_sizeSource != null && _sizeSource.Size != _size)
                return true;

            if (_scaleSource != null && _scaleSource.Scale != _scale)
                return true;

            if (_rotationSource != null && _rotationSource.Rotation != _rotation)
                return true;

            if (_rotationPivotSource != null &&
                _rotationPivotSource.RotationPivot != _rotationPivot)
                return true;

            if (_skewSource != null && _skewSource.Skew != _skew)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the position has changed.
    /// </summary>
    public bool IsPositionChanged =>
        _positionSource != null &&
        _positionSource.Position != _position;

    /// <summary>
    /// Gets a value indicating whether the size has changed.
    /// </summary>
    public bool IsSizeChanged =>
        _sizeSource != null &&
        _sizeSource.Size != _size;

    /// <summary>
    /// Gets a value indicating whether the scale has changed.
    /// </summary>
    public bool IsScaleChanged =>
        _scaleSource != null &&
        _scaleSource.Scale != _scale;

    /// <summary>
    /// Gets a value indicating whether the rotation has changed.
    /// </summary>
    public bool IsRotationChanged =>
        _rotationSource != null &&
        _rotationSource.Rotation != _rotation;

    /// <summary>
    /// Gets a value indicating whether the rotation pivot has changed.
    /// </summary>
    public bool IsRotationPivotChanged =>
        _rotationPivotSource != null &&
        _rotationPivotSource.RotationPivot != _rotationPivot;

    /// <summary>
    /// Gets a value indicating whether the skew has changed.
    /// </summary>
    public bool IsSkewChanged =>
        _skewSource != null &&
        _skewSource.Skew != _skew;

    /// <summary>
    /// Updates the tracked state with the current transform values.
    /// </summary>
    /// <remarks>
    /// After this method is called, the corresponding change properties return
    /// <see langword="false"/> until their source values change again.
    /// </remarks>
    public void UpdateState()
    {
        if (_positionSource != null)
            _position = _positionSource.Position;

        if (_sizeSource != null)
            _size = _sizeSource.Size;

        if (_scaleSource != null)
            _scale = _scaleSource.Scale;

        if (_rotationSource != null)
            _rotation = _rotationSource.Rotation;

        if (_rotationPivotSource != null)
            _rotationPivot = _rotationPivotSource.RotationPivot;

        if (_skewSource != null)
            _skew = _skewSource.Skew;
    }
}