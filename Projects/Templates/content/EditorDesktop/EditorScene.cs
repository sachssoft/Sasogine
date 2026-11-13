using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Scenes;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Minimal desktop editor with a workspace and a managed scene viewport.</summary>
public sealed class EditorScene : BasicSceneBase
{
    private Workspace? _workspace;

    /// <inheritdoc />
    public override void Load()
    {
        var root = new Grid { Id = "editor", Spacing = 4 };
        root.Rows.Add(new GridRow(GridLength.Auto));
        root.Rows.Add(new GridRow(GridLength.Full));
        root.Rows.Add(new GridRow(GridLength.Auto));
        root.Columns.Add(new GridColumn(240));
        root.Columns.Add(new GridColumn(6));
        root.Columns.Add(new GridColumn(GridLength.Full));

        var menu = new MenuBar();
        var file = new MenuItem { Text = "File" };
        file.Items.Add(new MenuItem { Text = "Exit", Action = () => Application.Exit() });
        menu.Items.Add(file);
        root.Place(menu, row: 0, column: 0, columnSpan: 3);

        var sidebar = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8 };
        sidebar.Children.Add(new TextBlock { Text = "Editor" });
        sidebar.Children.Add(new Button { Content = "Tool" });
        root.Place(sidebar, row: 1, column: 0);
        root.Place(new GridSplitter { Orientation = Orientation.Horizontal }, row: 1, column: 1);

        var viewport = new SceneViewport
        {
            Id = "editor.viewport",
            Render = DrawViewportScene,
            IsFocusable = true,
            Padding = Insets2.Zero,
            BorderThickness = 0
        };
        root.Place(viewport, row: 1, column: 2);

        var status = new StatusBar();
        status.Add(new StatusStrip { Content = "Ready" });
        root.Place(status, row: 2, column: 0, columnSpan: 3);

        _workspace = new Workspace(root);
        _workspace.SetSkin(SkinPreset.SachssoftDark);
        _workspace.Initialize((GameApplicationBase)Application);
    }

    /// <inheritdoc />
    public override void Update(SceneUpdateContext context)
    {
        base.Update(context);
        var bounds = context.GraphicsDevice.Viewport.Bounds;
        if (bounds.Width > 0 && bounds.Height > 0)
            _workspace?.Update(context, bounds);
    }

    /// <inheritdoc />
    public override void Draw(SceneDrawContext context)
    {
        context.GraphicsDevice.Clear(new Color(36, 36, 40));
        _workspace?.Draw(context);
    }

    /// <inheritdoc />
    public override void Unload()
    {
        _workspace?.Dispose();
        _workspace = null;
        base.Unload();
    }

    private static void DrawViewportScene(SceneViewportDrawContext viewport)
    {
        viewport.GetSceneContext().GraphicsDevice.Clear(Color.CornflowerBlue);
    }
}
