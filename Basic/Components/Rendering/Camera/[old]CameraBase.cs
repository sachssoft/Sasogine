//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;

//namespace Sachssoft.Sasogine.Components.Rendering.Camera
//{
//    /// <summary>
//    /// Base class for 2D or 3D cameras, providing transformation matrices
//    /// and screen-to-world/world-to-screen conversion.
//    /// </summary>
//    public abstract class CameraBase : ICamera
//    {
//        private Viewport _viewport;
//        private Matrix _world = Matrix.Identity;
//        private Matrix _projection;
//        private Matrix _view;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="CameraBase"/> class.
//        /// </summary>
//        /// <param name="graphicsDevice">Graphics device used for viewport operations.</param>
//        public CameraBase()
//        {
//        }

//        /// <summary>
//        /// Gets the projection matrix of the camera.
//        /// </summary>
//        public Matrix Projection
//        {
//            get => _projection;
//            private set => _projection = value;
//        }

//        /// <summary>
//        /// Gets the view matrix of the camera.
//        /// </summary>
//        public Matrix View
//        {
//            get => _view;
//            private set => _view = value;
//        }

//        /// <summary>
//        /// Gets the world matrix of the camera.
//        /// </summary>
//        public Matrix World
//        {
//            get => _world;
//            private set => _world = value;
//        }

//        protected Viewport Viewport => _viewport;

//        public void ApplyViewport(Viewport viewport)
//        {
//            _viewport = viewport;
//        }

//        /// <summary>
//        /// Applies the transformation matrices from another <see cref="ICameraTransform"/>.
//        /// </summary>
//        /// <param name="source">Source camera transform to apply.</param>
//        public void ApplyTransform(ICameraTransform source)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));

//            Projection = source.Projection;
//            View = source.View;
//            World = source.World;
//        }

//        /// <summary>
//        /// Converts screen coordinates to world coordinates.
//        /// </summary>
//        /// <param name="screenPosition">Screen position in pixels.</param>
//        /// <returns>Corresponding world coordinates.</returns>
//        public virtual Vector2 ToWorld(Vector2 screenPosition)
//        {
//            var unprojected = _viewport.Unproject(
//                new Vector3(screenPosition, 0f),
//                _projection,
//                _view,
//                _world
//            );
//            return new Vector2(unprojected.X, unprojected.Y);
//        }

//        /// <summary>
//        /// Converts world coordinates to screen coordinates.
//        /// </summary>
//        /// <param name="worldPosition">World position.</param>
//        /// <returns>Corresponding screen coordinates in pixels.</returns>
//        public virtual Vector2 ToScreen(Vector2 worldPosition)
//        {
//            var projected = _viewport.Project(
//                new Vector3(worldPosition, 0f),
//                _projection,
//                _view,
//                _world
//            );
//            return new Vector2(projected.X, projected.Y);
//        }

//        protected abstract Matrix ProjectionOverride();

//        protected abstract Matrix ViewOverride();

//        protected abstract Matrix WorldOverride();

//        protected void UpdateMatrices()
//        {
//            Projection = ProjectionOverride();
//            View = ViewOverride();
//            World = WorldOverride();
//        }

//        /// <summary>
//        /// Updates the camera state.
//        /// Override in derived classes to implement animation, movement, etc.
//        /// </summary>
//        /// <param name="context">Frame context containing elapsed time.</param>
//        public virtual void Update(GameContext context)
//        {
//            UpdateMatrices();
//        }

//        /// <summary>
//        /// Determines whether a bounding box is within the camera's view frustum.
//        /// </summary>
//        /// <param name="min">Minimum corner of the bounding box.</param>
//        /// <param name="max">Maximum corner of the bounding box.</param>
//        /// <param name="world">Optional additional world transform.</param>
//        /// <returns>True if the bounding box intersects the camera's view frustum.</returns>
//        public virtual bool IsInView(Vector3 min, Vector3 max, Matrix? world = null)
//        {
//            var worldMatrix = world ?? Matrix.Identity;

//            var box = new BoundingBox(min, max);

//            if (worldMatrix != Matrix.Identity)
//                box = TransformBoundingBox(box, worldMatrix);

//            var frustum = new BoundingFrustum(World * View * Projection);
//            return frustum.Intersects(box);
//        }

//        /// <summary>
//        /// Transforms a bounding box by a given matrix.
//        /// </summary>
//        /// <param name="box">Bounding box to transform.</param>
//        /// <param name="matrix">Transformation matrix.</param>
//        /// <returns>Transformed bounding box.</returns>
//        private static BoundingBox TransformBoundingBox(BoundingBox box, Matrix matrix)
//        {
//            Vector3[] corners = box.GetCorners();
//            for (int i = 0; i < corners.Length; i++)
//            {
//                corners[i] = Vector3.Transform(corners[i], matrix);
//            }
//            return BoundingBox.CreateFromPoints(corners);
//        }

//        Matrix ICameraTransform.Projection
//        {
//            get => Projection;
//            //set => throw new InvalidOperationException("Cannot set Projection directly. Use camera methods to update it.");
//        }

//        Matrix ICameraTransform.View
//        {
//            get => View;
//            //set => throw new InvalidOperationException("Cannot set Scene directly. Use camera methods to update it.");
//        }

//        Matrix ICameraTransform.World
//        {
//            get => World;
//            //set => throw new InvalidOperationException("Cannot set World directly. Use camera methods to update it.");
//        }

//        //void ICameraTransform.CopyTo(ICameraTransform target)
//        //{
//        //    throw new InvalidOperationException("Copying from this camera is not allowed externally.");
//        //}
//    }
//}
