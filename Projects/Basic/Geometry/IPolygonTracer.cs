using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Defines a backend for tracing raster texture data into vector contours.
/// </summary>
public interface IPolygonTracer
{
    /// <summary>
    /// Traces the specified texture into vector contours.
    /// </summary>
    /// <param name="texture">The texture to trace.</param>
    /// <param name="options">The options that control the tracing process.</param>
    /// <returns>The generated tracing result.</returns>
    PolygonTraceResult Trace(
        Texture2D texture,
        PolygonTraceOptions options);
}
