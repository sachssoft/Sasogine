

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sachssoft.Graphics.Primitives;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Graphics.Renderer;
using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine.Tiling.Composite;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public class EditorToolFill : EditorToolCursor
{
    private Rectangle _map_size;
    private Coordinate _cursor_position;

    public Coordinate CursorPosition => _cursor_position;

    public ITileMapFloodFiller? FloodFiller { get; set; }

    public override void Update(TileEditorRuntimeBase runtime, GameContext context)
    {
        base.Update(runtime, context);

        if (runtime?.Map == null)
            return;

        var tile_size = runtime.TileSize;
        var world_pos = runtime.Camera.ToWorld(runtime.Axis.Get(TileEditorCursorAxisInputs.Move));
        var tile_pos = Vector2.Round(world_pos / tile_size);
        var tile_coord = new Coordinate((int)tile_pos.X, (int)tile_pos.Y);

        _map_size = Rectangle.Empty;

        if (tile_coord.IsOutOfBorder(runtime.Map.Columns, runtime.Map.Rows))
            return;

        _map_size = new Rectangle(0, 0, runtime.Map.Columns, runtime.Map.Rows);
        _cursor_position = tile_coord;

        if (IsInteracting)
        {
            FloodFiller?.Fill(_cursor_position);
        }
        else if (WasJustReleased)
        {
            ResetInteractionState();
        }
    }

    public override void Draw(GameContext context, TilePrimitive primitive, TileRenderer renderer, Texture2D cursor_texture)
    {
        if (!IsInteracting && !WasJustReleased)
        {
            if (_map_size.Contains(_cursor_position.ToPoint()))
                renderer.DrawTile(primitive, cursor_texture, _cursor_position, new()
                {
                    Color = Color.LightPink
                });
            return;
        }
    }

    //    private bool _mouse_pressed = false;

    //    public EditorFill()
    //    {
    //    }

    //    protected override void OnUpdate(GameContext context, BenBallRuntime runtime)
    //    {
    //        if (context.InputEvents.Mouse.LeftButton == ButtonState.Pressed && _mouse_pressed == false)
    //        {
    //            _mouse_pressed = true;

    //            var c = CursorRectangle.Location;
    //            var level = runtime.Level!;
    //            var tile = (EditorLayeredTile)level.Tiles[c.X, c.Y];

    //            FloodFill((EditorLayeredTile[,])level.Tiles, c.X, c.Y, tile.Ground, new PlatformGround());

    //            //Fill(runtime, c.X, c.Y, typeof(PlatformGround));

    //            //if (runtime.Level != null)
    //            //{
    //            //    var layer = (EditorTileLayer)_selected_level.World.Tiles[c.X, c.Y];

    //            //    FloodFillUtil.Fill(
    //            //        (EditorTileLayer[,])_selected_level.World.Tiles,
    //            //        _selected_level.World.Columns,
    //            //        _selected_level.World.Rows,
    //            //        c.X,
    //            //        c.Y,
    //            //        layer.Middle.GetType(),
    //            //        _selected_tile_type,
    //            //        layer => (TileBase)layer.Middle,
    //            //        (layer, tile) => layer.Middle = tile);
    //            //}
    //        }
    //        else if (context.InputEvents.Mouse.LeftButton == ButtonState.Released && _mouse_pressed == true)
    //        {
    //            _mouse_pressed = false;
    //        }
    //    }

    //private void FloodFill(int[,] map, int start_x, int start_y, int target_value, int replacement_value)
    //{
    //    if (target_value == replacement_value)
    //        return;

    //    int width = map.GetLength(0);
    //    int height = map.GetLength(1);

    //    if (map[start_x, start_y] != target_value)
    //        return;

    //    Queue<(int x, int y)> queue = new();
    //    queue.Enqueue((start_x, start_y));

    //    while (queue.Count > 0)
    //    {
    //        var (x, y) = queue.Dequeue();

    //        if (x < 0 || y < 0 || x >= width || y >= height)
    //            continue;

    //        if (map[x, y] != target_value)
    //            continue;

    //        map[x, y] = replacement_value;

    //        queue.Enqueue((x + 1, y));
    //        queue.Enqueue((x - 1, y));
    //        queue.Enqueue((x, y + 1));
    //        queue.Enqueue((x, y - 1));
    //    }
    //}

}