using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls.Styling;

internal sealed class ControlSkinRegistration
{
    internal ControlSkinRegistration(ControlSkin skin) => Skin = skin;

    internal ControlSkinRegistration(Func<GraphicsDevice, ControlSkin> factory) => Factory = factory;

    internal Func<GraphicsDevice, ControlSkin>? Factory { get; }

    internal ControlSkin? Skin { get; set; }
}
