namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Exercises actual window resizing and checks the processed layout and rendered DPI agree.</summary>
internal sealed class InspectorResizeChecks
{
    private readonly (int Width, int Height, float Dpi)[] _steps;
    private int _step;
    private bool _applied;
    internal InspectorResizeChecks(float finalDpi) => _steps = new[]
    {
        (1360, 900, 96f), (1600, 1000, 96f), (1000, 760, 96f), (1360, 900, 144f), (1360, 900, finalDpi)
    };
    internal bool IsComplete => _step >= _steps.Length;
    internal void BeforeUpdate(GameApplicationBase application, Workspace workspace)
    {
        if (IsComplete || _applied) return;
        var step = _steps[_step];
        application.ChangeResolution(step.Width, step.Height);
        workspace.Dpi = step.Dpi;
        _applied = true;
    }
    internal void AfterDraw(LogoInspectorView view, ControlDrawContext context)
    {
        if (IsComplete) return;
        var step = _steps[_step];
        float scale = step.Dpi / 96f;
        if (context.Viewport.Width != step.Width || context.Viewport.Height != step.Height || context.Scale != scale
            || MathF.Abs(view.Workspace.Size.Width - step.Width / scale) > .01f
            || MathF.Abs(view.Workspace.Size.Height - step.Height / scale) > .01f
            || MathF.Abs(view.Workspace.Content.Bounds.Width - view.Workspace.Size.Width) > .01f
            || MathF.Abs(view.TitleEditor.Bounds.Height - 32) > .01f)
            throw new InvalidOperationException("Window resize, logical layout and rendered DPI do not agree.");
        Console.WriteLine($"PASS: Window {step.Width}x{step.Height}, DPI {step.Dpi}: logical layout {view.Workspace.Size.Width:0.##}x{view.Workspace.Size.Height:0.##}; fixed text height 32.");
        _step++; _applied = false;
    }
}
