namespace Sachssoft.Sasogine.Diagnostics;

public class DiagnosticsContext
{
    public IDebugDisplay? DebugDisplay { get; set; }

    public DiagnosticsDisplayFlags Flags { get; set; } =
        DiagnosticsDisplayFlags.FPS;
}
