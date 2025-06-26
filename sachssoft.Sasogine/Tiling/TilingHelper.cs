using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sachssoft.Sasogine.Graphics;
using System;

namespace sachssoft.Sasogine.Tiling;

public static class TilingHelper
{
    public static Coordinate GetCursor(Vector2 position, Vector2 tile_size, CameraBase camera)
    {
        var world_pos = camera.ToWorld(position);
        return new Coordinate((int)float.Round(world_pos.X / tile_size.X), (int)float.Round(world_pos.Y / tile_size.Y));
    }

    public static Coordinate GetCursorClamped(ITileMap map, Vector2 position, Vector2 tile_size, CameraBase camera)
    {
        return Coordinate.Clamp(GetCursor(position, tile_size, camera), Coordinate.Zero, new Coordinate(map.Columns, map.Rows));
    }

    public static bool IsOutOfBorder(ITileMap map, Coordinate coordinate)
    {
        return coordinate.IsOutOfBorder(map.Columns, map.Rows);
    }

    public static Rectangle GetViewBounds(CameraBase camera, Vector2 tile_size)
        => GetViewBounds(IMyGameApp.Current.GraphicsDevice.Viewport, camera, tile_size);

    public static Rectangle GetViewBounds(GraphicsDevice graphics_device, CameraBase camera, Vector2 tile_size)
        => GetViewBounds(graphics_device.Viewport, camera, tile_size);

    public static Rectangle GetViewBounds(Viewport viewport, CameraBase camera, Vector2 tile_size)
    {
        int viewport_width = viewport.Width;
        int viewport_height = viewport.Height;

        Vector2 top_left = camera.ToWorld(new Vector2(0, 0));
        Vector2 bottom_right = camera.ToWorld(new Vector2(viewport_width, viewport_height));

        int x = (int)Math.Min(top_left.X, bottom_right.X);
        int y = (int)Math.Min(top_left.Y, bottom_right.Y);
        int width = (int)(Math.Abs(bottom_right.X - top_left.X) + tile_size.X);
        int height = (int)(Math.Abs(bottom_right.Y - top_left.Y) + tile_size.Y);

        return new Rectangle(x, y, width, height);
    }
}
