//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Sachssoft.Sasogine.Components.Rendering.Animation;
//using Sachssoft.Sasogine.Components.Rendering.Animation.Timings;
//using System;

//namespace Sachssoft.Sasogine.Components.Rendering.Camera
//{
//    /// <summary>
//    /// Hybrid camera supporting both 2D Orthographic and 3D Perspective rendering.
//    /// </summary>
//    public class Camera3D : CameraBase
//    {
//        // --- Common 2D/3D ---
//        private Vector3 _position = Vector3.Zero;
//        private Vector3 _target = Vector3.Zero;
//        private Vector3 _up = Vector3.Up;

//        private bool _is3D = false;

//        // 2D properties
//        private float _zoom = 1f;
//        private Vector2 _origin = Vector2.Zero;

//        // 2D Plane for orthographic
//        private float _plane_minimum = -10f;
//        private float _plane_maximum = 10f;

//        // 3D perspective
//        private float _fov = MathHelper.PiOver4;
//        private float _nearPlane = 0.1f;
//        private float _farPlane = 1000f;

//        // Animation (optional)
//        private Vector3 _move_start, _move_end;
//        private float _move_time, _move_duration;
//        private AnimationTimingBase? _move_timing;
//        private bool _move_animating;

//        public Camera3D() : base()
//        {
//        }

//        #region Properties

//        public bool Is3D
//        {
//            get => _is3D;
//            set => _is3D = value;
//        }

//        public Vector3 Position
//        {
//            get => _position;
//            set => _position = value;
//        }

//        public Vector3 Target
//        {
//            get => _target;
//            set => _target = value;
//        }

//        public Vector3 Up
//        {
//            get => _up;
//            set => _up = value;
//        }

//        public Vector2 Origin
//        {
//            get => _origin;
//            set => _origin = value;
//        }

//        public float Zoom
//        {
//            get => _zoom;
//            set => _zoom = MathHelper.Max(value, 0.0001f);
//        }

//        public float PlaneMinimum
//        {
//            get => _plane_minimum;
//            set => _plane_minimum = value;
//        }

//        public float PlaneMaximum
//        {
//            get => _plane_maximum;
//            set => _plane_maximum = value;
//        }

//        public float Fov
//        {
//            get => _fov;
//            set => _fov = MathHelper.Clamp(value, 0.01f, MathHelper.Pi - 0.01f);
//        }

//        public float NearPlane
//        {
//            get => _nearPlane;
//            set => _nearPlane = MathHelper.Max(0.01f, value);
//        }

//        public float FarPlane
//        {
//            get => _farPlane;
//            set => _farPlane = MathHelper.Max(_nearPlane + 0.01f, value);
//        }

//        #endregion

//        #region Animation

//        public bool IsMoveAnimating => _move_animating;

//        public void MoveTo(Vector3 targetPosition, float duration, AnimationTimingBase? timing = null)
//        {
//            _move_start = _position;
//            _move_end = targetPosition;
//            _move_duration = MathF.Max(duration, 0.001f);
//            _move_time = 0f;
//            _move_timing = timing ?? new EaseOutQuad();
//            _move_animating = true;
//        }

//        #endregion

//        #region Update

//        protected override Matrix ProjectionOverride()
//        {
//            return _is3D ? Matrix.CreatePerspectiveFieldOfView(
//                                     _fov,
//                                     GraphicsDevice.Viewport.AspectRatio,
//                                     _nearPlane,
//                                     _farPlane)
//                               : Matrix.CreateOrthographic(
//                                     GraphicsDevice.Viewport.Width * _zoom,
//                                     GraphicsDevice.Viewport.Height * _zoom,
//                                     _plane_minimum,
//                                     _plane_maximum);
//        }

//        protected override Matrix ViewOverride()
//        {
//            return _is3D ? Matrix.CreateLookAt(_position, _target, _up)
//                         : Matrix.CreateTranslation(-_origin.X, -_origin.Y, 0f);
//        }

//        protected override Matrix WorldOverride()
//        {
//            return Matrix.Identity;
//        }

//        public override void Update(GameContext context)
//        {
//            float dt = (float)context.GameTime.ElapsedGameTime.TotalSeconds;

//            // Animation
//            if (_move_animating && _move_timing != null)
//            {
//                _move_time += dt;
//                float t = MathF.Min(_move_time / _move_duration, 1f);
//                t = _move_timing.GetValue(t);

//                _position = Vector3.Lerp(_move_start, _move_end, t);

//                if (_move_time >= _move_duration)
//                    _move_animating = false;
//            }

//            //// Projection & View
//            //Projection = _is3D ? Matrix.CreatePerspectiveFieldOfView(
//            //                         _fov,
//            //                         GraphicsDevice.Viewport.AspectRatio,
//            //                         _nearPlane,
//            //                         _farPlane)
//            //                   : Matrix.CreateOrthographic(
//            //                         GraphicsDevice.Viewport.Width * _zoom,
//            //                         GraphicsDevice.Viewport.Height * _zoom,
//            //                         _plane_minimum,
//            //                         _plane_maximum);

//            //View = _is3D ? Matrix.CreateLookAt(_position, _target, _up)
//            //             : Matrix.CreateTranslation(-_origin.X, -_origin.Y, 0f);

//            //World = Matrix.Identity;

//            base.Update(context);
//        }

//        #endregion

//        #region World/Screen Conversion

//        public Vector3 ToWorld(Vector3 screenPosition)
//        {
//            Vector3 ndc = new Vector3(
//                2f * screenPosition.X / GraphicsDevice.Viewport.Width - 1f,
//                1f - 2f * screenPosition.Y / GraphicsDevice.Viewport.Height,
//                screenPosition.Z);

//            Matrix inv = Matrix.Invert(View * Projection);
//            return Vector3.Transform(ndc, inv);
//        }

//        public Vector3 ToScreen(Vector3 worldPosition)
//        {
//            Vector3 ndc = Vector3.Transform(worldPosition, View * Projection);
//            return new Vector3(
//                (ndc.X + 1f) * 0.5f * GraphicsDevice.Viewport.Width,
//                (1f - ndc.Y) * 0.5f * GraphicsDevice.Viewport.Height,
//                ndc.Z);
//        }

//        #endregion
//    }
//}
