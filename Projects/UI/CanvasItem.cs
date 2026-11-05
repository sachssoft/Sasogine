using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Hosts arbitrary content with independent position, size, scale, rotation and pivot.</summary>
/// <remarks>Size participates in content layout; Scale transforms the laid-out content.
/// Rotation uses radians, and RotationPivot uses normalized item coordinates.</remarks>
public class CanvasItem : ItemContainer, IControlTransform, ITransformPosition2,
    ITransformSize2, ITransformScale2, ITransformRotation2, ITransformRotationPivot2
{
    /// <summary>Identifies changes to AllowMove.</summary>
    private static readonly PropertyChangedEventArgs AllowMoveArgs = new(nameof(AllowMove));

    /// <summary>Identifies changes to AllowPivot.</summary>
    private static readonly PropertyChangedEventArgs AllowPivotArgs = new(nameof(AllowPivot));

    /// <summary>Identifies changes to AllowResize.</summary>
    private static readonly PropertyChangedEventArgs AllowResizeArgs = new(nameof(AllowResize));

    /// <summary>Identifies changes to AllowRotate.</summary>
    private static readonly PropertyChangedEventArgs AllowRotateArgs = new(nameof(AllowRotate));

    /// <summary>Identifies changes to AllowScale.</summary>
    private static readonly PropertyChangedEventArgs AllowScaleArgs = new(nameof(AllowScale));

    /// <summary>Identifies changes to IsLocked.</summary>
    private static readonly PropertyChangedEventArgs IsLockedArgs = new(nameof(IsLocked));

    /// <summary>Identifies position changes.</summary>
    private static readonly PropertyChangedEventArgs PositionArgs = new(nameof(Position));

    /// <summary>Identifies rotation changes.</summary>
    private static readonly PropertyChangedEventArgs RotationArgs = new(nameof(Rotation));

    /// <summary>Identifies pivot changes.</summary>
    private static readonly PropertyChangedEventArgs RotationPivotArgs = new(nameof(RotationPivot));

    /// <summary>Identifies scale changes.</summary>
    private static readonly PropertyChangedEventArgs ScaleArgs = new(nameof(Scale));

    /// <summary>Identifies size changes.</summary>
    private static readonly PropertyChangedEventArgs SizeArgs = new(nameof(Size));

    /// <summary>Stores whether AllowMove is enabled.</summary>
    private bool _allowMove = true;

    /// <summary>Stores whether AllowPivot is enabled.</summary>
    private bool _allowPivot = true;

    /// <summary>Stores whether AllowResize is enabled.</summary>
    private bool _allowResize = true;

    /// <summary>Stores whether AllowRotate is enabled.</summary>
    private bool _allowRotate = true;

    /// <summary>Stores whether AllowScale is enabled.</summary>
    private bool _allowScale = true;

    /// <summary>Stores whether IsLocked is enabled.</summary>
    private bool _isLocked;

    /// <summary>Stores the item's rotation in radians.</summary>
    private float _rotation;

    /// <summary>Stores the layout position relative to the canvas content origin.</summary>
    private Point2 _position;

    /// <summary>Stores the normalized rotation pivot.</summary>
    private Point2 _rotationPivot = new(.5f, .5f);

    /// <summary>Stores positive visual scale factors.</summary>
    private Vector2 _scale = Vector2.One;

    /// <summary>Creates an empty editable content host.</summary>
    public CanvasItem() : this(null)
    {
    }

    /// <summary>Creates an editable presenter for a data item.</summary>
    /// <param name="item">The item supplied by an item template.</param>
    public CanvasItem(object? item) : base(item)
    {
        Width = 100;
        Height = 100;
    }

    /// <summary>Gets or sets the item's layout position within its canvas.</summary>
    public Point2 Position
    {
        get => _position;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (SetNotify(ref _position, value, PositionArgs))
                Parent?.InvalidateMeasure();
        }
    }

    /// <summary>Gets or sets the unscaled layout dimensions through Width and Height.</summary>
    public Size2 Size
    {
        get => new(Width ?? 100, Height ?? 100);
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value.Width) || !float.IsFinite(value.Height) || value.Width <= 0 || value.Height <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            Width = value.Width;
            Height = value.Height;
        }
    }

    /// <summary>Gets or sets positive visual scale factors without changing content layout size.</summary>
    public Vector2 Scale
    {
        get => _scale;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || value.X < .001f || value.Y < .001f)
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _scale, value, ScaleArgs);
        }
    }

    /// <summary>Gets or sets the visual rotation in radians.</summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _rotation, value, RotationArgs);
        }
    }

    /// <summary>Gets or sets the normalized pivot. Values outside the item are supported.</summary>
    public Point2 RotationPivot
    {
        get => _rotationPivot;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
                throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _rotationPivot, value, RotationPivotArgs);
        }
    }

    /// <summary>Gets or sets whether editor gestures are disabled for this item.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _isLocked, value, IsLockedArgs);
        }
    }

    /// <summary>Gets or sets whether the canvas can move this item.</summary>
    public bool AllowMove
    {
        get => _allowMove;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _allowMove, value, AllowMoveArgs);
        }
    }

    /// <summary>Gets or sets whether the canvas can change the item's layout size.</summary>
    public bool AllowResize
    {
        get => _allowResize;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _allowResize, value, AllowResizeArgs);
        }
    }

    /// <summary>Gets or sets whether the canvas can change the item's visual scale.</summary>
    public bool AllowScale
    {
        get => _allowScale;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _allowScale, value, AllowScaleArgs);
        }
    }

    /// <summary>Gets or sets whether rotation gestures are enabled.</summary>
    public bool AllowRotate
    {
        get => _allowRotate;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _allowRotate, value, AllowRotateArgs);
        }
    }

    /// <summary>Gets or sets whether the pivot handle can be moved.</summary>
    public bool AllowPivot
    {
        get => _allowPivot;
        set
        {
            VerifyTreeMutation();
            SetNotify(ref _allowPivot, value, AllowPivotArgs);
        }
    }

    /// <inheritdoc/>
    public Matrix VisualTransform => GetVisualTransform(Bounds);

    /// <summary>Builds the visual transform for specified layout bounds.</summary>
    /// <param name="bounds">The actual or proposed layout bounds.</param>
    /// <returns>The absolute layout-space visual transformation.</returns>
    internal Matrix GetVisualTransform(Bounds2 bounds)
    {
        float x = bounds.X + bounds.Width * RotationPivot.X;
        float y = bounds.Y + bounds.Height * RotationPivot.Y;
        return Matrix.CreateTranslation(-x, -y, 0)
                * Matrix.CreateScale(Scale.X, Scale.Y, 1)
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateTranslation(x, y, 0);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        base.OnPropertyChanged(notification);
        if (notification.PropertyName is nameof(Width) or nameof(Height))
            NotifyPropertyChanged(SizeArgs);
        Parent?.InvalidateMeasure();
    }
}
