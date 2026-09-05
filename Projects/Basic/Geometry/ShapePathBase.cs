namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Provides a base implementation for geometric shapes represented by a
/// generated <see cref="Path"/>.
/// </summary>
public abstract class ShapePathBase
{
    private Path? _definedPath;

    /// <summary>
    /// Gets the path that defines the geometry of this shape.
    /// </summary>
    /// <remarks>
    /// The path is built on first access and cached until
    /// <see cref="Rebuild"/> is called.
    /// </remarks>
    public Path DefinedPath
    {
        get
        {
            _definedPath ??= BuildDefinedPath();
            return _definedPath;
        }
    }

    /// <summary>
    /// Rebuilds the path that defines the geometry of this shape.
    /// </summary>
    public void Rebuild()
    {
        _definedPath = BuildDefinedPath();
    }

    /// <summary>
    /// Builds the path that defines the geometry of this shape.
    /// </summary>
    /// <returns>
    /// The generated path representing the shape geometry.
    /// </returns>
    protected abstract Path BuildDefinedPath();
}