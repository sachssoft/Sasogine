using Sachssoft.Engine.Controls.Examples;
RenderValidationApplication.HalfPixelMode=args.Contains("--half-pixel");
using var app=new RenderValidationApplication();
app.Run();
