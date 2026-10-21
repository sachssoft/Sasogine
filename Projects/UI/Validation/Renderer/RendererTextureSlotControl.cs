using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Exercises ordered translucent texture slots, rollover and mipmapped minification.</summary>
internal sealed class RendererTextureSlotControl(Texture2D[] textures) : Control
{
    protected override void OnDraw(ControlDrawContext context)
    {
        for(int pass=0;pass<4;pass++)
            for(int i=0;i<textures.Length;i++)
            {
                context.DrawTexture(textures[i],new Rectangle(0,0,32,32),
                    new Bounds2(20+i*65+pass*13,30+pass*110,150,150),Color.White*.7f);
                context.DrawTexture(textures[i],new Rectangle(0,0,32,32),
                    new Bounds2(25+i*35,560+pass*15,5,5),Color.White*.8f);
            }
    }
}
