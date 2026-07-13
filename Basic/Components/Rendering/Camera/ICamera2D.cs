using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Basic.Components.Rendering.Camera
{
    public interface ICamera2D : ICamera, ICameraTransform
    {

        Vector2 Position { get; set; }

        Vector2 PositionMinimum { get; set; }

        Vector2 PositionMaximum { get; set; }

        float Zoom { get; set; }

        float BaseZoomFactor { get; set; }

        float ZoomMinimum { get; set; }

        float ZoomMaximum { get; set; }

    }
}
