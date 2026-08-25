using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Vector2 ToWorld(Vector2 screenPosition);

        /// <summary>
        /// Converts a world-space position into screen-space coordinates.
        /// </summary>
        Vector2 ToScreen(Vector2 worldPosition);

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