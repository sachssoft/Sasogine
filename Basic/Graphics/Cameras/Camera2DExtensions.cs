using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Camera;

namespace Sachssoft.Sasogine.Graphics.Cameras
{
    public static class Camera2DExtensions
    {
        /// <summary>
        /// Determines whether a bounding box is within the camera's view frustum.
        /// </summary>
        /// <param name="min">Minimum corner of the bounding box.</param>
        /// <param name="max">Maximum corner of the bounding box.</param>
        /// <param name="world">Optional additional world transform.</param>
        /// <returns>True if the bounding box intersects the camera's view frustum.</returns>
        public static bool IsInView(this ICamera camera, Vector3 min, Vector3 max, Matrix? world = null)
        {
            var worldMatrix = world ?? Matrix.Identity;

            var box = new BoundingBox(min, max);

            if (worldMatrix != Matrix.Identity)
                box = TransformBoundingBox(box, worldMatrix);

            var frustum = new BoundingFrustum(camera.World * camera.View * camera.Projection);
            return frustum.Intersects(box);
        }

        private static BoundingBox TransformBoundingBox(BoundingBox box, Matrix matrix)
        {
            Vector3[] corners = box.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Vector3.Transform(corners[i], matrix);
            }
            return BoundingBox.CreateFromPoints(corners);
        }
    }
}
