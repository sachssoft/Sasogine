/* 
 * © 2024 Tobias Sachs
 * GameContext
 * 16.06.2026
 * Update: 
*/

using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Graphics;

namespace sachssoft.Sasogine;

public class MultiScreenGameContext : GameContext
{
    public MultiScreenGameContext(GameContext self, CameraBase camera, Rectangle viewport, int player_index)
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
