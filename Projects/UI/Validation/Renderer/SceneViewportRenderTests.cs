using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Compares direct viewports to images, including inherited clipping, nested targets and UI painter order.</summary>
internal static class SceneViewportRenderTests
{
    internal static void Run(GameApplicationBase app, IControlFont font)
    {
        var device = app.GraphicsDevice;
        if (device.UseHalfPixelOffset)
        {
            Console.WriteLine("SceneViewport: the editor retains its image path for half-pixel addressing.");
            return;
        }
        using var white = new Texture2D(device,1,1); white.SetData(new[]{Color.White});
        using var batch = new SpriteBatch(device);
        using var clipped = new RasterizerState { CullMode=CullMode.None,ScissorTestEnable=true };
        using var cache = new RenderTarget2D(device,16,16,false,SurfaceFormat.Color,DepthFormat.None,0,RenderTargetUsage.PreserveContents);
        using var target = new RenderTarget2D(device,1160,760,false,SurfaceFormat.Color,DepthFormat.None,0,RenderTargetUsage.PreserveContents);
        foreach(float scale in new[]{1f,1.25f})
        foreach(bool partial in new[]{false,true})
        foreach(bool nonPremultiplied in new[]{false,true})
        {
            int width=(int)(300*scale),height=(int)(200*scale);
            using var image = new RenderTarget2D(device,width,height,false,SurfaceFormat.Color,DepthFormat.None,0,RenderTargetUsage.PreserveContents);
            void Geometry(bool nested)
            {
                int w=device.Viewport.Width,h=device.Viewport.Height;
                batch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,clipped);
                batch.Draw(white,new Rectangle(10,10,w/2,h/2),Color.Red);
                batch.End();
                if(nested)
                {
                    var original=device.GetRenderTargets();var vp=device.Viewport;var scissor=device.ScissorRectangle;
                    device.SetRenderTarget(cache);device.Clear(Color.Green);
                    device.SetRenderTargets(original);device.Viewport=vp;device.ScissorRectangle=scissor;
                }
                batch.Begin(SpriteSortMode.Deferred,nonPremultiplied ? BlendState.NonPremultiplied : BlendState.AlphaBlend,SamplerState.PointClamp,DepthStencilState.None,clipped);
                batch.Draw(white,new Rectangle(w/4,h/4,w/2,h/2),Color.Blue*.5f);
                batch.End();
            }
            device.SetRenderTarget(image);device.ScissorRectangle=image.Bounds;device.Clear(Color.Black);Geometry(false);
            device.SetRenderTarget(null);
            Workspace Make(bool direct)
            {
                Control view=direct ? new SceneViewport { Width=300,Height=200,Background=Color.Black,UseDirectRendering=true,CompositeBackground=nonPremultiplied ? new Color(30,45,60) : null,Render=_=>Geometry(true) } :
                    new Image { Width=300,Height=200,Source=image,Stretch=ImageStretch.Fill };
                var panel=new Panel { Width=partial?240:400,Height=partial?160:300,Margin=new Insets2(40,30,0,0),HorizontalAlignment=ControlAlignment.Start,VerticalAlignment=ControlAlignment.Start };
                view.Margin=new Insets2(20,10,0,0);view.HorizontalAlignment=ControlAlignment.Start;view.VerticalAlignment=ControlAlignment.Start;
                panel.Add(view);
                panel.Add(new TextBlock { Text="UI overlay",Width=140,Height=30,Background=Color.Yellow,TextColor=Color.Black,Margin=new Insets2(80,80,0,0),HorizontalAlignment=ControlAlignment.Start,VerticalAlignment=ControlAlignment.Start });
                return new Workspace(new Panel { Background=new Color(30,45,60), Children={panel} });
            }
            Color[] Capture(bool direct)
            {
                using var root=Make(direct);root.Update(new ControlInputFrame(new Size2(1160/scale,760/scale),TimeSpan.Zero),new ControlLayoutContext(font));
                using var draw=new ControlDrawContext(app,font,new Rectangle(0,0,1160,760),scale);
                draw.SetFrameTime(new GameTime(TimeSpan.FromSeconds(10),TimeSpan.Zero));
                device.SetRenderTarget(target);device.Clear(Color.Magenta);root.Draw(draw);
                var pixels=new Color[1160*760];target.GetData(pixels);device.SetRenderTarget(null);return pixels;
            }
            Color[] a=Capture(false),b=Capture(true);int differences=0,max=0;
            for(int i=0;i<a.Length;i++)
            {
                if(a[i]==b[i])continue;differences++;
                int d=Math.Max(Math.Max(Math.Abs(a[i].R-b[i].R),Math.Abs(a[i].G-b[i].G)),Math.Max(Math.Abs(a[i].B-b[i].B),Math.Abs(a[i].A-b[i].A)));
                max=Math.Max(max,d);
            }
            Console.WriteLine($"SceneViewport scale={scale} partial={partial} nonPremultiplied={nonPremultiplied}: differences={differences}, max={max}");
            if(differences!=0)throw new InvalidOperationException("Direct viewport differs from the image reference.");
        }
        using(var root=new Workspace(new SceneViewport { UseDirectRendering=true,Render=_=> {device.SetRenderTarget(cache);throw new InvalidOperationException("Expected scene failure");} }))
        using(var draw=new ControlDrawContext(app,font,new Rectangle(0,0,1160,760)))
        {
            draw.SetFrameTime(new GameTime(TimeSpan.FromSeconds(10),TimeSpan.Zero));
            root.Update(new ControlInputFrame(new Size2(1160,760),TimeSpan.Zero),new ControlLayoutContext(font));
            device.SetRenderTarget(target);var viewport=device.Viewport;var scissor=device.ScissorRectangle;
            var blend=device.BlendState;var depth=device.DepthStencilState;var rasterizer=device.RasterizerState;
            bool failed=false;
            try {root.Draw(draw);}catch(InvalidOperationException ex)when(ex.Message=="Expected scene failure"){failed=true;}
            if(!failed || device.RenderTargetCount!=1 || !ReferenceEquals(device.GetRenderTargets()[0].RenderTarget,target) ||
                !device.Viewport.Equals(viewport) || device.ScissorRectangle!=scissor || !ReferenceEquals(device.BlendState,blend) ||
                !ReferenceEquals(device.DepthStencilState,depth) || !ReferenceEquals(device.RasterizerState,rasterizer))
                throw new InvalidOperationException("Scene exception leaked graphics state.");
            device.SetRenderTarget(null);
        }
        Console.WriteLine("PASS: direct scene/image equality, DPI, partial clipping, nested targets, UI overlap and exception restoration.");
    }
}
