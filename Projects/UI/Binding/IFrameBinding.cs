namespace Sachssoft.Engine.Controls.Binding;
/// <summary>
/// Updates one explicit binding at the deterministic frame boundary.
/// </summary>
public interface IFrameBinding
{
    /// <summary>
    /// Performs update using the current control state.
    /// </summary>
    void Update();
}
