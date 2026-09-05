using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Cameras;
using System;

namespace Sachssoft.Sasogine.Components.Rendering.Cameras
{
    /// <summary>
    /// Provides a two-dimensional camera implementation with position,
    /// zoom, rotation and coordinate conversion support.
    /// The camera owns and manages its transformation matrices and updates
    /// them automatically when camera properties change.
    /// </summary>
    public class Camera2 : EngineObject<Camera2Definition>, ICamera2
    {
        private const float ZoomMinConstant = 0.01f;
        private const float ScaleMinConstant = 0.001f;

        private Viewport _viewport;

        private Point2 _position;
        private float _zoom = 1f;
        private float _rotation;

        private float _baseZoomFactor = 1f;

        private Point2 _positionMinimum =
            new(float.MinValue, float.MinValue);

        private Point2 _positionMaximum =
            new(float.MaxValue, float.MaxValue);

        private float _zoomMinimum = 0.001f;
        private float _zoomMaximum = float.MaxValue;

        private float _rotationMinimum = float.MinValue;
        private float _rotationMaximum = float.MaxValue;

        private Matrix _projection = Matrix.Identity;
        private Matrix _view = Matrix.Identity;
        private Matrix _world = Matrix.Identity;

        /// <summary>
        /// Initializes a new camera with default settings.
        /// </summary>
        public Camera2()
            : this(new Camera2Definition())
        {
        }

        /// <summary>
        /// Initializes a new camera using the specified definition.
        /// </summary>
        /// <param name="definition">
        /// The camera definition used to initialize the camera.
        /// </param>
        public Camera2(Camera2Definition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Gets or sets the current world position of the camera.
        /// </summary>
        public Point2 Position
        {
            get => _position;
            set
            {
                var position = new Point2(
                    MathHelper.Clamp(
                        value.X,
                        PositionMinimum.X,
                        PositionMaximum.X),
                    MathHelper.Clamp(
                        value.Y,
                        PositionMinimum.Y,
                        PositionMaximum.Y));

                if (_position == position)
                    return;

                _position = position;
                UpdateMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed camera position.
        /// </summary>
        public Point2 PositionMinimum
        {
            get => _positionMinimum;
            set
            {
                _positionMinimum = value;
                Position = _position;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed camera position.
        /// </summary>
        public Point2 PositionMaximum
        {
            get => _positionMaximum;
            set
            {
                _positionMaximum = value;
                Position = _position;
            }
        }

        /// <summary>
        /// Gets or sets the current camera zoom factor.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set
            {
                var zoom = MathHelper.Clamp(
                    value,
                    ZoomMinimum,
                    ZoomMaximum);

                if (Math.Abs(_zoom - zoom) < float.Epsilon)
                    return;

                _zoom = zoom;
                UpdateMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the base world scaling factor.
        /// </summary>
        public virtual float BaseZoomFactor
        {
            get => _baseZoomFactor;
            set
            {
                _baseZoomFactor = Math.Max(
                    ScaleMinConstant,
                    value);

                UpdateMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed zoom value.
        /// </summary>
        public float ZoomMinimum
        {
            get => _zoomMinimum;
            set
            {
                _zoomMinimum = Math.Max(
                    ZoomMinConstant,
                    value);

                if (_zoom < _zoomMinimum)
                {
                    _zoom = _zoomMinimum;
                    UpdateMatrices();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed zoom value.
        /// </summary>
        public float ZoomMaximum
        {
            get => _zoomMaximum;
            set
            {
                _zoomMaximum = Math.Max(
                    _zoomMinimum,
                    value);

                if (_zoom > _zoomMaximum)
                {
                    _zoom = _zoomMaximum;
                    UpdateMatrices();
                }
            }
        }

        /// <summary>
        /// Gets or sets the camera rotation in radians.
        /// </summary>
        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = MathHelper.Clamp(
                    value,
                    RotationMinimum,
                    RotationMaximum);

                UpdateMatrices();
            }
        }

        /// <summary>
        /// Gets or sets the minimum allowed rotation in radians.
        /// </summary>
        public float RotationMinimum
        {
            get => _rotationMinimum;
            set
            {
                _rotationMinimum = value;
                Rotation = _rotation;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed rotation in radians.
        /// </summary>
        public float RotationMaximum
        {
            get => _rotationMaximum;
            set
            {
                _rotationMaximum = value;
                Rotation = _rotation;
            }
        }

        /// <summary>
        /// Gets the projection matrix of the camera.
        /// </summary>
        public virtual Matrix Projection => _projection;

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        public virtual Matrix View => _view;

        /// <summary>
        /// Gets the world matrix of the camera.
        /// </summary>
        public virtual Matrix World => _world;

        /// <summary>
        /// Applies the specified viewport to the camera and recalculates
        /// its transformation matrices.
        /// </summary>
        /// <param name="viewport">
        /// The viewport used by the camera.
        /// </param>
        public void ApplyViewport(Viewport viewport)
        {
            if (viewport.Width <= 0 || viewport.Height <= 0)
                return;

            _viewport = viewport;
            UpdateMatrices();
        }

        /// <summary>
        /// Updates the camera state.
        /// </summary>
        /// <param name="context">
        /// The current game context.
        /// </param>
        public virtual void Update(GameContext context)
        {
            UpdateMatrices();
        }

        /// <summary>
        /// Restores the camera to its definition values.
        /// </summary>
        public virtual void Reset()
        {
            ApplyDefinition();
        }

        /// <summary>
        /// Configures the camera from its definition.
        /// </summary>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            ApplyDefinition();
        }

        /// <summary>
        /// Applies values from the camera definition.
        /// </summary>
        protected virtual void ApplyDefinition()
        {
            Position = Definition.Position;
            Zoom = Definition.Zoom;
            Rotation = Definition.Rotation;
        }

        /// <summary>
        /// Recalculates the camera transformation matrices.
        /// </summary>
        protected virtual void UpdateMatrices()
        {
            if (_viewport.Width <= 0 || _viewport.Height <= 0)
                return;

            _projection =
                Matrix.CreateOrthographicOffCenter(
                    0,
                    _viewport.Width,
                    _viewport.Height,
                    0,
                    -1,
                    1);

            var center = new Vector2(
                _viewport.Width * 0.5f,
                _viewport.Height * 0.5f);

            _view =
                Matrix.CreateTranslation(
                    -Position.X,
                    -Position.Y,
                    0f)
                *
                Matrix.CreateTranslation(
                    center.X,
                    center.Y,
                    0f)
                *
                Matrix.CreateRotationZ(Rotation)
                *
                Matrix.CreateScale(
                    BaseZoomFactor * Zoom,
                    BaseZoomFactor * Zoom,
                    1f)
                *
                Matrix.CreateTranslation(
                    -center.X,
                    -center.Y,
                    0f);
        }

        /// <summary>
        /// Converts a world-space position into screen-space coordinates.
        /// </summary>
        /// <param name="worldPosition">
        /// The position in world space.
        /// </param>
        /// <returns>
        /// The corresponding position in screen space.
        /// </returns>
        public virtual Point2 ToScreen(Point2 worldPosition)
        {
            var result = _viewport.Project(
                new Vector3(
                    worldPosition.X,
                    worldPosition.Y,
                    0f),
                Projection,
                View,
                World);

            return new Point2(
                result.X,
                result.Y);
        }

        /// <summary>
        /// Converts a screen-space position into world-space coordinates.
        /// </summary>
        /// <param name="screenPosition">
        /// The position in screen space.
        /// </param>
        /// <returns>
        /// The corresponding position in world space.
        /// </returns>
        public virtual Point2 ToWorld(Point2 screenPosition)
        {
            var result = _viewport.Unproject(
                new Vector3(
                    screenPosition.X,
                    screenPosition.Y,
                    0f),
                Projection,
                View,
                World);

            return new Point2(
                result.X,
                result.Y);
        }

        /// <summary>
        /// Creates a copy of the camera including its current state.
        /// </summary>
        /// <returns>
        /// A new camera containing the same definition and current state.
        /// </returns>
        public virtual ICamera Clone()
        {
            var camera = new Camera2(
                new Camera2Definition
                {
                    Position = Position,
                    Zoom = Zoom,
                    Rotation = Rotation
                });

            camera.BaseZoomFactor = BaseZoomFactor;

            camera.PositionMinimum = PositionMinimum;
            camera.PositionMaximum = PositionMaximum;

            camera.ZoomMinimum = ZoomMinimum;
            camera.ZoomMaximum = ZoomMaximum;

            camera.RotationMinimum = RotationMinimum;
            camera.RotationMaximum = RotationMaximum;

            camera.ApplyViewport(_viewport);

            return camera;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}