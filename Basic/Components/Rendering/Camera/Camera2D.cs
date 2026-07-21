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
    public class Camera2D : EngineObject<Camera2DDefinition>, ICamera2D
    {
        private const float ZoomMinConstant = 0.01f;
        private const float ScaleMinConstant = 0.001f;

        private Viewport _viewport;

        private Vector2 _position;
        private float _zoom = 1f;
        private float _rotation;

        private float _baseZoomFactor = 1f;

        private Vector2 _positionMinimum =
            new(float.MinValue, float.MinValue);

        private Vector2 _positionMaximum =
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
        public Camera2D()
            : this(new Camera2DDefinition())
        {
        }


        /// <summary>
        /// Initializes a new camera using the specified definition.
        /// </summary>
        public Camera2D(Camera2DDefinition definition)
            : base(definition)
        {
        }


        /// <summary>
        /// Gets or sets the current world position of the camera.
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set
            {
                var position = Vector2.Clamp(
                    value,
                    PositionMinimum,
                    PositionMaximum);

                if (_position == position)
                    return;

                _position = position;
                UpdateMatrices();
            }
        }


        /// <summary>
        /// Gets or sets the minimum allowed camera position.
        /// </summary>
        public Vector2 PositionMinimum
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
        public Vector2 PositionMaximum
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
                _zoomMinimum = Math.Max(ZoomMinConstant, value);

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
                _zoomMaximum = Math.Max(_zoomMinimum, value);

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
        /// Gets or sets the minimum allowed rotation.
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
        /// Gets or sets the maximum allowed rotation.
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
        /// Applies the current viewport to the camera.
        /// </summary>
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
        public virtual void Update(GameContext context)
        {
            UpdateMatrices();
        }


        /// <summary>
        /// Resets the camera to its definition values.
        /// </summary>
        public virtual void Reset()
        {
            ApplyDefinition();
        }


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
        /// Recalculates the camera matrices.
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
        /// Converts a world position into screen coordinates.
        /// </summary>
        public virtual Vector2 ToScreen(Vector2 worldPosition)
        {
            return _viewport.Project(
                new Vector3(worldPosition, 0),
                Projection,
                View,
                World)
                .ToVector2();
        }


        /// <summary>
        /// Converts a screen position into world coordinates.
        /// </summary>
        public virtual Vector2 ToWorld(Vector2 screenPosition)
        {
            return _viewport.Unproject(
                new Vector3(screenPosition, 0),
                Projection,
                View,
                World)
                .ToVector2();
        }


        /// <summary>
        /// Creates a copy of the camera including its current state.
        /// </summary>
        public virtual ICamera Clone()
        {
            var camera = new Camera2D(
                new Camera2DDefinition
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