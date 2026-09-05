using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Graphics.Cameras
{
    /// <summary>
    /// Defines a camera that provides transformation data and camera operations.
    /// </summary>
    public interface ICamera : ICameraTransform, ICloneable
    {
        /// <summary>
        /// Updates the camera viewport and recalculates projection data.
        /// </summary>
        void ApplyViewport(Viewport viewport);

        /// <summary>
        /// Converts a screen-space position into world-space coordinates.
        /// </summary>
        /// <param name="screenPosition">
        /// The position in screen space.
        /// </param>
        /// <returns>
        /// The corresponding position in world space.
        /// </returns>
        Point2 ToWorld(Point2 screenPosition);

        /// <summary>
        /// Converts a world-space position into screen-space coordinates.
        /// </summary>
        /// <param name="worldPosition">
        /// The position in world space.
        /// </param>
        /// <returns>
        /// The corresponding position in screen space.
        /// </returns>
        Point2 ToScreen(Point2 worldPosition);

        /// <summary>
        /// Updates the camera state.
        /// </summary>
        void Update(GameContext context);

        /// <summary>
        /// Restores the camera to its default state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates a copy of this camera.
        /// </summary>
        new ICamera Clone();
    }
}