using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Exercises overlap, alpha, rotated clipping, atlas crops, custom drawing, and font fallback.</summary>
internal sealed class RendererGeometryControl(Texture2D image,IControlFont spriteFont,IControlFont legacyFont) : Control
{
    protected override void OnDraw(ControlDrawContext c)
    {
        c.FillRectangle(new Bounds2(0,0,Bounds.Width,Bounds.Height),new Color(30,40,60));
        for(int i=0;i<80;i++)
        {
            c.FillRectangle(new Bounds2(20+i*7,20+i%9*3,35,90),new Color(80,140,210,200));
            c.DrawTexture(image,new Rectangle(0,0,8,8),new Bounds2(25+i*7,60+i%11*2,18,55),Color.White*.65f);
        }
        using(c.PushClip(new Bounds2(17.35f,155.6f,285.1f,150.2f)))
        {
            c.DrawLine(new Vector2(-20,140),new Vector2(350,330),Color.Orange,13);
            c.DrawLine(new Vector2(30,350),new Vector2(240,110),Color.Cyan,6);
            c.DrawTexture(image,new Rectangle(2,1,5,6),new Bounds2(-20,160,170,150),Color.White);
            c.DrawText(c.DefaultFont,"Clipped alpha / FontStashSharp",new Vector2(-10,260),Color.White);
        }
        c.DrawText(spriteFont,"AB? AB\n B?A",new Vector2(340,170),Color.LightGreen);
        c.DrawText(legacyFont,"Legacy font bridge",new Vector2(340,260),Color.White);
        c.FillRectangle(new Bounds2(335,275,180,20),new Color(160,80,40,220));
        // Raw SpriteBatch access is an explicit barrier, including when native geometry follows.
        var compatibilityBatch=c.SpriteBatch;
        compatibilityBatch.Draw(image,new Vector2(520,180),null,Color.White*.7f,0,Vector2.Zero,new Vector2(6),SpriteEffects.FlipHorizontally,0);
        c.FillRectangle(new Bounds2(530,190,60,25),new Color(30,200,100,180));
        using(c.PushClipScope(new Bounds2(510,170,90,80)))
        {
            c.FillRectangle(new Bounds2(550,230,30,15),Color.Yellow);
            compatibilityBatch.Draw(image,new Vector2(580,180),Color.White);
        }
        compatibilityBatch.Draw(image,new Vector2(610,180),Color.White);
        if(c.Renderer==ControlRenderer.FrameBatch)
            c.FrameBatch.Draw(image,new Vector2(640,200),null,Color.White,.4f,new Vector2(4),new Vector2(9),SpriteEffects.FlipVertically,.35f);
        else
            c.SpriteBatch.Draw(image,new Vector2(640,200),null,Color.White,.4f,new Vector2(4),new Vector2(9),SpriteEffects.FlipVertically,.35f);
    }
}
