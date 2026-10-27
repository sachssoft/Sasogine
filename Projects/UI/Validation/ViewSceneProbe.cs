using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;

/// <summary>Provides a view scene for lifecycle and camera-free contract checks.</summary>
internal sealed class ViewSceneProbe : ViewSceneBase
{
    /// <summary>Creates a probe with a caller-owned test font.</summary>
    /// <param name="font">The shared measurement and drawing font.</param>
    internal ViewSceneProbe(IControlFont font) : base(font)
    {
    }

    /// <inheritdoc/>
    protected override Workspace CreateWorkspace() => new(new TextBlock { Text = "Camera-free scene" });
}
