using Sachssoft.Engine.Controls.Examples;

RenderValidationApplication.HalfPixelMode = args.Contains("--half-pixel");
RenderValidationApplication.CarouselBenchmark = args.Contains("--carousel-benchmark");
using var app = new RenderValidationApplication();
app.Run();
