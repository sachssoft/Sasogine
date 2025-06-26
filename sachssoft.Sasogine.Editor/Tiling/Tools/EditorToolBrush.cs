using Microsoft.Xna.Framework.Input;
using sachssoft.Sasogine.Tiling;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public class EditorToolBrush : EditorToolCursor
{
    private bool _was_pressed = false;

    public EditorToolBrush()
    {
    }

    public override void Update(TileEditorRuntimeBase runtime, GameContext context)
    {
        base.Update(runtime, context);

        // Drücken halten
        //if (runtime.CursorButton == ButtonState.Pressed)
        if (runtime.Interaction.IsPressed(TileEditorCursorInteractions.ToolCursor))
        {
            if (!_was_pressed)
            {
                _was_pressed = true;
            }

            for (int tx = Scope.Lower.X; tx <= Scope.Upper.X; tx++)
            {
                for (int ty = Scope.Lower.Y; ty <= Scope.Upper.Y; ty++)
                {
                    PaintAt(new Coordinate(tx, ty), runtime);
                }
            }
        }
        // Loslassen, nur einmal
        //else if (runtime.CursorButton == ButtonState.Released && _was_pressed)
        else if (runtime.Interaction.WasJustPressed(TileEditorCursorInteractions.ToolCursor) && _was_pressed)
        {
            _was_pressed = false;
            FinishPainting(runtime);
        }
    }

    protected virtual void PaintAt(Coordinate coordinate, TileEditorRuntimeBase runtime)
    {
        runtime.ToolCursorAction?.Execute(this, coordinate);
    }

    protected virtual void FinishPainting(TileEditorRuntimeBase runtime)
    {
        runtime.ToolCursorAction?.Complete();
    }
}
