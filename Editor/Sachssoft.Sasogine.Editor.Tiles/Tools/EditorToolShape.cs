using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Tiling;
using System;

namespace Sachssoft.Sasogine.Editor.Tiles.Tools;

public class EditorToolShape : EditorToolCursor
{
    private Rectangle _map_size;
    private Coordinate _start_tile;
    private Coordinate _end_tile;
    private Coordinate _cursor_position;
    private Scope _scope;
    private Coordinate[]? _output;

    public IEditorToolShapeGeometry Shape { get; }

    public SelectionDrawMode DrawMode { get; set; } = SelectionDrawMode.Filled;

    public override Scope Scope => _scope;

    public EditorToolShape(IEditorToolShapeGeometry shape)
    {
        Shape = shape ?? throw new ArgumentNullException(nameof(shape));
    }

    public override void Update(TileEditorRuntimeBase runtime, GameFrameContext context)
    {
        base.Update(runtime, context);

        if (runtime?.Map == null)
            return;

        var tile_size = runtime.TileSize;
        var world_pos = runtime.Camera.ToWorld(runtime.Axis.Get(TileEditorCursorAxisInputs.Move));
        var tile_pos = Vector2.Round(world_pos / tile_size);
        var tile_coord = new Coordinate((int)tile_pos.X, (int)tile_pos.Y);

        _map_size = new Rectangle(0, 0, runtime.Map.Columns, runtime.Map.Rows);

        if (IsInteracting)
        {
            _end_tile = tile_coord;

            if (_scope.IsZero()) // Startpunkt setzen, wenn Selektion beginnt
                _start_tile = _end_tile;

            _scope = Scope.FromInclusive(_start_tile, _end_tile);
        }
        else if (WasJustReleased)
        {
            ResetInteractionState();
            // Auswahl abgeschlossen – du könntest hier ein Event feuern oder Selektion zurücksetzen

            if (_output != null)
            {
                for (int i = 0; i < _output.Length; i++)
                {
                    runtime.ToolCursorAction?.Execute(this, _output[i]);
                }
                _output = null;
            }
        }
        else
        {
            _scope = Scope.Zero; // Kein aktives Rechteck
            _cursor_position = tile_coord;
        }
    }

    public override void Draw(GameFrameContext context, TilePrimitive primitive, TileRenderer renderer, Texture2D cursor_texture)
    {
        if (Shape == null || (Shape is not IEditorToolShapePath && Shape is not IEditorToolShapeRegion))
            return;

        if (!IsInteracting && !WasJustReleased)
        {
            if (_map_size.Contains(_cursor_position.ToPoint()))
                renderer.DrawTile(primitive, cursor_texture, _cursor_position, new()
                {
                    Color = Color.LightBlue
                });
            return;
        }

        if (_scope.IsZero())
            return;

        var size = _scope.ToSize();
        _output = new Coordinate[(size.X + 1) * (size.Y + 1)];
        int count = 0;

        for (int y = 0; y <= size.Y; y++)
        {
            for (int x = 0; x <= size.X; x++)
            {
                var coord = new Coordinate(_scope.Lower.X + x, _scope.Lower.Y + y);
                bool drawable = false;

                if (Shape is IEditorToolShapeRegion region)
                {
                    drawable = (DrawMode == SelectionDrawMode.Outline && region.ContainsOutline(coord, _scope, Size)) ||
                               (DrawMode == SelectionDrawMode.Filled && region.ContainsFill(coord, _scope));
                }
                else if (Shape is IEditorToolShapePath path)
                {
                    drawable = path.Contains(coord, _start_tile, _end_tile, Size);
                }

                if (drawable)
                {
                    _output[count++] = coord;

                    renderer.DrawTile(primitive, cursor_texture, coord, new()
                    {
                        Color = Color.LightBlue * 0.5f
                    });
                }
            }
        }

        // Kürze das Array auf die tatsächliche Anzahl der genutzten Elemente (count),
        // indem ein neues Array mit den ersten 'count' Elementen erstellt wird
        _output = _output[..count];
    }
}
