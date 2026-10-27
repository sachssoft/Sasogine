# ViewSceneBase

`Sachssoft.Engine.Controls.ViewSceneBase` implements `IScene` directly. It lives in the
Controls assembly and hosts one `Workspace`, without creating a world camera or material.

```csharp
public sealed class MenuScene : ViewSceneBase
{
    private Button _start = null!;

    public MenuScene(IControlFont font) : base(font)
    {
        BackgroundColor = Color.Black;
    }

    protected override Workspace CreateWorkspace()
    {
        _start = new Button { Content = "Start" };
        var workspace = new Workspace(_start);
        // Configure workspace styles, fonts, DPI and an optional skin here.
        return workspace;
    }

    protected override void OnUpdate(SceneUpdateContext context)
    {
        if (_start.IsPressed)
        {
            // Start the application-specific action.
        }
    }
}
```

Pass the scene to the existing `SceneManager`. It calls `Enter`, `Load`, `Update` and
`Draw`. No custom camera factory or separate application-side UI draw is required.

- `CreateWorkspace()` runs once per load. `Application` is available there.
- Input and layout run before `OnUpdate`, so frame flags can be polled there.
- The physical viewport and `Workspace.Dpi` drive sizing and drawing consistently.
  Set DPI explicitly through the workspace when the host's DPI changes.
- Native text input is subscribed only while active; mouse, keyboard and touch are sampled
  once each update. Override `UpdateWorkspace` to supply host snapshots or replay frames.
- `Exit` deactivates input and closes transient interactions. Persistent scenes keep their
  workspace until unloaded. Re-entering resets sampler history.
- `Unload` releases the workspace, bindings and drawing resources. Reloading builds a new
  tree. `Dispose` is final. Application shutdown also unloads the active view scene.
- Fonts and skins remain caller-owned and must outlive their use by the scene. Returning
  a workspace transfers ownership to the scene. Clean up resources created by a failing
  `CreateWorkspace` inside that factory.

`IScene.RequiresCamera` defaults to `true` for existing scenes. View scenes return `false`
and expose exactly one view. Their `SceneUpdateContext.Cameras` array is empty and
`SceneDrawContext.HasCamera` is false. Reading `ViewCamera` or `DefaultMaterial` on such a
context throws `InvalidOperationException`; custom transitions must support this case.
The existing camera-based constructor and rendering path remain available.
