namespace Sachssoft.Engine.Controls.Input;
/// <summary>
/// Represents control input context.
/// </summary>
public sealed class ControlInputContext
{
    internal ControlInputContext(Workspace workspace)
    {
        Workspace = workspace;
    }

    /// <summary>
    /// Gets the workspace processing this event.
    /// </summary>
    public Workspace Workspace { get; }
    /// <summary>
    /// Gets the identity of the pointer currently being dispatched.
    /// </summary>
    public ControlPointer Pointer { get; internal set; }

    /// <summary>
    /// Assigns keyboard focus, or clears it when null.
    /// </summary>
    public void Focus(Control? control) => Workspace.Focus(control);
    /// <summary>
    /// Captures pointer events for an available control.
    /// </summary>
    public void CapturePointer(Control control) => Workspace.CapturePointer(control, Pointer);
    /// <summary>
    /// Releases capture if held by the specified control.
    /// </summary>
    public void ReleasePointer(Control control) => Workspace.ReleasePointer(control, Pointer);
    /// <summary>
    /// Captures an explicitly identified pointer.
    /// </summary>
    public void CapturePointer(Control control, ControlPointer pointer) => Workspace.CapturePointer(control, pointer);
    /// <summary>
    /// Releases an explicitly identified pointer.
    /// </summary>
    public void ReleasePointer(Control control, ControlPointer pointer) => Workspace.ReleasePointer(control, pointer);
}
