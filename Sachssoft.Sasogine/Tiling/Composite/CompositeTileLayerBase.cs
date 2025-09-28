using Sachssoft.Sasogine.Graphics.Rendering;
using System;

namespace Sachssoft.Sasogine.Tiling.Composite;

public abstract class CompositeTileBase : ICloneable, IOutputTile
{
    internal object? _map_source = null;

    public CompositeTileBase() { }

    internal void SetMapSource(object? map_source)
    {
        _map_source = map_source;
        OnMapChanged();
    }

    public CompositeTileMap<T>? GetMap<T>() where T : CompositeTileBase
        => (CompositeTileMap<T>?)_map_source;

    protected virtual void OnMapChanged() { }

    public abstract CompositeTileBase Clone();

    object ICloneable.Clone() => Clone();

    internal protected virtual void OnEnter(Coordinate coordinate)
    {
    }

    internal protected virtual void OnLeave()
    {
    }

    public virtual void Update(GameFrameContext context)
    {
        // Logik für Update, falls nötig
    }

    public virtual void Draw(in CompositeTileRenderArgs args, TileDrawingOptions options)
    {
        // Logik für das Zeichnen – args enthält Context, Renderer, Primitive, Coordinate
    }

    bool IOutputTile.Equals(IOutputTile other)
    {
        return Equals(other);
    }
}
