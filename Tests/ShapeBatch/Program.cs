using Sachssoft.Sasogine.ShapeBatchTests;

GeometryChecks.Run();
if (args.Contains("--gpu"))
{
    using var app = new RenderChecks();
    app.Run();
}
