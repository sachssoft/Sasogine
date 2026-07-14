using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Sasogine.Tiling;
using System;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Editor.Tiles.Tools;

public class EditorToolCursor
{
    private bool _is_interacting = false;
    private bool _was_just_released = false;

    private Point _map_size;
    private Point _cursor_position;
    private Scope _scope;
    private bool _is_cursor_inside;

    // Farbe des Cursors
    public Color Color { get; set; } = Color.Red;

    // Größe des Cursors in Kacheln
    public ushort Size { get; set; } = 1;

    // Bereich des Cursors - Koordinantenrechteck
    public virtual Scope Scope => _scope;

    public bool IsInteracting => _is_interacting; // = IsPressed

    public bool WasJustReleased => _was_just_released; // = IsReleased

    public Point MapSize => _map_size;

    public object? DataContext { get; set; }

    public virtual void Update(TileEditorRuntimeBase runtime, GameFrameContext context)
    {
        if (runtime?.Map == null)
            return;

        var tile_size = runtime.TileSize;
        var world_pos = runtime.Camera.ToWorld(runtime.Axis.Get(TileEditorCursorAxisInputs.Move));
        var half_brush = Size / 2f;

        _map_size = new Point(runtime.Map.Columns, runtime.Map.Rows);

        // Die Cursor-Position wird in Tile-Koordinaten umgerechnet.
        // Bei ungerader BrushSize (z.B. 1x1, 3x3, 5x5, ...) wird die Position gerundet, damit der Cursor zentriert zur Maus liegt.
        // Bei gerader BrushSize (z.B. 2x2, 4x4, ...) wird die Position aufgerundet, damit der Cursor konsistent rechts/unten ausgerichtet ist.
        var tile_pos = (Size % 2 != 0)
            ? Vector2.Round(world_pos / tile_size)
            : Vector2.Ceiling(world_pos / tile_size);

        _cursor_position = new Point((int)tile_pos.X, (int)tile_pos.Y);

        var xmin = _cursor_position.X - (int)MathF.Floor(Size / 2f);
        var ymin = _cursor_position.Y - (int)MathF.Floor(Size / 2f);
        var xmax = xmin + Size - 1;
        var ymax = ymin + Size - 1;

        var map_cols = runtime.Map.Columns;
        var map_rows = runtime.Map.Rows;

        _is_cursor_inside =
            !(xmax < 0 || ymax < 0 || xmin >= map_cols || ymin >= map_rows);

        if (!_is_cursor_inside)
            return;

        var tx_start = int.Max(xmin, 0);
        var ty_start = int.Max(ymin, 0);
        var tx_end = int.Min(xmax, map_cols - 1);
        var ty_end = int.Min(ymax, map_rows - 1);

        _scope = new Scope(tx_start, ty_start, tx_end, ty_end);
    }

    public virtual void Draw(GameFrameContext context, TilePrimitive primitive, TileRenderer renderer, Texture2D cursor_texture)
    {
        if (!_is_cursor_inside)
            return;

        var size = _scope.ToSize();

        for (int y = 0; y <= size.Y; y++)
        {
            for (int x = 0; x <= size.X; x++)
            {
                var coord = new Coordinate(
                    _scope.Lower.X + x,
                    _scope.Lower.Y + y);

                renderer.DrawTile(primitive, cursor_texture, coord, new()
                {
                    Color = Color
                });
            }
        }
    }

    public void BeginInteraction()
    {
        _is_interacting = true;
    }

    public void EndInteraction()
    {
        _is_interacting = false;
        _was_just_released = true;
    }

    public void ResetInteractionState()
    {
        _is_interacting = false;
        _was_just_released = false;
    }
}
