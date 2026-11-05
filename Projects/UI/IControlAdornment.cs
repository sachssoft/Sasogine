namespace Sachssoft.Engine.Controls;

/// <summary>Draws editor adornments after a control's children using its local drawing coordinates.</summary>
public interface IControlAdornment
{
    /// <summary>Draws the current selection overlay without changing layout or interaction state.</summary>
    /// <param name="context">The active local drawing context.</param>
    void DrawAdornment(ControlDrawContext context);
}
