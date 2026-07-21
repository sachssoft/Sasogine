using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Rendering.Cameras;
using System;

namespace Sachssoft.Sasogine.Graphics.Cameras
{
    /// <summary>
    /// Provides extension methods for camera calculations.
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Determines whether a world-space bounding box intersects
        /// the camera's visible view area.
        /// </summary>
        /// <param name="camera">
        /// The camera used for the visibility test.
        /// </param>
        /// <param name="min">
        /// The minimum corner of the bounding box in world space.
        /// </param>
        /// <param name="max">
        /// The maximum corner of the bounding box in world space.
        /// </param>
        /// <param name="world">
        /// An optional world transformation applied before the visibility test.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the bounding box intersects the camera view;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsInView(
            this ICamera camera,
            Vector3 min,
            Vector3 max,
            Matrix? world = null)
        {
            var worldMatrix = world ?? Matrix.Identity;

            var box = new BoundingBox(min, max);

            if (worldMatrix != Matrix.Identity)
                box = TransformBoundingBox(box, worldMatrix);

            var frustum = new BoundingFrustum(
                camera.World *
                camera.View *
                camera.Projection);

            return frustum.Intersects(box);
        }

        /// <summary>
        /// Calculates the visible world-space bounds of the camera viewport.
        /// </summary>
        /// <param name="camera2D">
        /// The 2D camera used to transform screen coordinates into world coordinates.
        /// </param>
        /// <param name="screenViewport">
        /// The viewport area representing the visible screen region.
        /// </param>
        /// <returns>
        /// A <see cref="Box"/> containing the minimum and maximum world-space coordinates
        /// visible through the camera.
        /// </returns>
        public static Box GetWorldBounds(
            this ICamera2D camera2D,
            Viewport screenViewport)
        {
            var inverseView = Matrix.Invert(camera2D.View);

            Vector2 topLeft = Vector2.Transform(
                Vector2.Zero,
                inverseView);

            Vector2 topRight = Vector2.Transform(
                new Vector2(screenViewport.Width, 0),
                inverseView);

            Vector2 bottomLeft = Vector2.Transform(
                new Vector2(0, screenViewport.Height),
                inverseView);

            Vector2 bottomRight = Vector2.Transform(
                new Vector2(
                    screenViewport.Width,
                    screenViewport.Height),
                inverseView);

            float minX = float.Min(
                float.Min(topLeft.X, topRight.X),
                float.Min(bottomLeft.X, bottomRight.X));

            float minY = float.Min(
                float.Min(topLeft.Y, topRight.Y),
                float.Min(bottomLeft.Y, bottomRight.Y));

            float maxX = float.Max(
                float.Max(topLeft.X, topRight.X),
                float.Max(bottomLeft.X, bottomRight.X));

            float maxY = float.Max(
                float.Max(topLeft.Y, topRight.Y),
                float.Max(bottomLeft.Y, bottomRight.Y));

            return new Box(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Converts a screen-space position into world-space coordinates.
        /// </summary>
        /// <param name="camera">
        /// The 2D camera used for the transformation.
        /// </param>
        /// <param name="position">
        /// The position in screen space.
        /// </param>
        /// <returns>
        /// The corresponding position in world space.
        /// </returns>
        public static Vector2 GetWorldPosition(
            this ICamera2D camera,
            Vector2 position)
        {
            var inverseView = Matrix.Invert(camera.View);

            return Vector2.Transform(
                position,
                inverseView);
        }

        /// <summary>
        /// Determines whether a world-space position is contained within the camera viewport.
        /// </summary>
        /// <param name="camera">
        /// The 2D camera used for the visibility calculation.
        /// </param>
        /// <param name="position">
        /// The world-space position to check.
        /// </param>
        /// <param name="screenViewport">
        /// The viewport area used to calculate the visible world bounds.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the position is inside the camera view;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool ContainsWorldPosition(
            this ICamera2D camera,
            Vector2 position,
            Viewport screenViewport)
        {
            var bounds = camera.GetWorldBounds(screenViewport);

            return bounds.Contains(position);
        }

        private static BoundingBox TransformBoundingBox(
            BoundingBox box,
            Matrix matrix)
        {
            Vector3[] corners = box.GetCorners();

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Vector3.Transform(
                    corners[i],
                    matrix);
            }

            return BoundingBox.CreateFromPoints(corners);
        }
    }
}