/* 
 * © 2024 Tobias Sachs
 * GameContext
 * 16.06.2026
 * Update: 
*/

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine;

public class MultiScreenGameContext : GameFrameContext
{
    public MultiScreenGameContext(GameFrameContext self, CameraBase camera, Rectangle viewport, int player_index)
        : base(self)
    {
        Camera = camera;
        Viewport = viewport;
        PlayerIndex = player_index;
    }

    public CameraBase Camera { get; }

    public Rectangle Viewport { get; }

    public int PlayerIndex { get; }
}
