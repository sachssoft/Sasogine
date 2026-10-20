using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Forces repeated buffer rollover while alternating disjoint textured geometry.</summary>
internal sealed class RendererStressControl(Texture2D texture) : Control
{
    protected override void OnDraw(ControlDrawContext context)
    {
        for(int i=0;i<12000;i++)
        {
            float x=i%300*3,y=i/300*3;
            if((i&1)==0) context.FillRectangle(new Bounds2(x,y,2,2),Color.Orange);
            else context.DrawTexture(texture,new Rectangle(0,0,8,8),new Bounds2(x,y,2,2),Color.White);
        }
    }
}
