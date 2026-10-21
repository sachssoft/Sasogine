namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Runs ownership guards inside an actual Workspace.Draw traversal.</summary>
internal sealed class OwnershipDrawProbe : Control
{
    internal Action? Checking { get; set; }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context) => Checking?.Invoke();
}
