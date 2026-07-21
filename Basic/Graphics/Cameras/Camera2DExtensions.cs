using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Cameras
{
    /// <summary>
    /// Provides extension methods for camera visibility and view calculations.
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