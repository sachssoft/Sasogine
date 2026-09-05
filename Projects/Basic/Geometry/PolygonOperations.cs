using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry.Internal;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Provides high-level polygon operations such as stroking, triangulation,
/// clipping, simplification, transformation, and offset generation.
/// </summary>
public static class PolygonOperations
{
    /// <summary>
    /// Generates stroked polygon contours from the specified input contours.
    /// </summary>
    /// <param name="contours">
    /// The polygon contours to stroke.
    /// </param>
    /// <param name="options">
    /// The options that control stroke generation.
    /// </param>
    /// <param name="strokerBackend">
    /// The optional stroking backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The generated stroked polygon contours.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<Vector2>> Stroke(
        IReadOnlyList<IReadOnlyList<Vector2>> contours,
        PolygonStrokeOptions options,
        IPolygonStroker? strokerBackend = null)
    {
        strokerBackend ??= new Clipper2PolygonStroker();

        return strokerBackend.Stroke(
            contours,
            options);
    }

    /// <summary>
    /// Triangulates the specified polygon contours.
    /// </summary>
    /// <param name="contours">
    /// The polygon contours to triangulate.
    /// </param>
    /// <param name="options">
    /// The options that control triangulation.
    /// </param>
    /// <param name="triangulatorBackend">
    /// The optional triangulation backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The triangulation result containing the generated geometry.
    /// </returns>
    public static PolygonTriangulationResult Triangulate(
        IReadOnlyList<IReadOnlyList<Vector2>> contours,
        PolygonTriangulationOptions options,
        IPolygonTriangulator? triangulatorBackend = null)
    {
        triangulatorBackend ??= new LibTessPolygonTriangulator();

        return triangulatorBackend.Triangulate(
            contours,
            options);
    }

    /// <summary>
    /// Performs a clipping operation between the specified subject
    /// and clipping contours.
    /// </summary>
    /// <param name="subject">
    /// The subject polygon contours.
    /// </param>
    /// <param name="clip">
    /// The clipping polygon contours.
    /// </param>
    /// <param name="operation">
    /// The clipping operation to perform.
    /// </param>
    /// <param name="clipperBackend">
    /// The optional clipping backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The polygon contours produced by the clipping operation.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<Vector2>> Clip(
        IReadOnlyList<IReadOnlyList<Vector2>> subject,
        IReadOnlyList<IReadOnlyList<Vector2>> clip,
        PolygonClipOperation operation = PolygonClipOperation.Union,
        IPolygonClipper? clipperBackend = null)
    {
        clipperBackend ??= new Clipper2PolygonClipper();

        return clipperBackend.Clip(
            subject,
            clip,
            operation);
    }

    /// <summary>
    /// Simplifies the specified polygon contours.
    /// </summary>
    /// <param name="contours">
    /// The polygon contours to simplify.
    /// </param>
    /// <param name="options">
    /// The options that control polygon simplification.
    /// </param>
    /// <param name="simplifierBackend">
    /// The optional simplification backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The simplified polygon contours.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<Vector2>> Simplify(
        IReadOnlyList<IReadOnlyList<Vector2>> contours,
        PolygonSimplificationOptions options = default,
        IPolygonSimplifier? simplifierBackend = null)
    {
        simplifierBackend ??= new Clipper2PolygonSimplifier();

        return simplifierBackend.Simplify(
            contours,
            options);
    }

    /// <summary>
    /// Transforms the specified polygon contours using the provided matrix.
    /// </summary>
    /// <param name="contours">
    /// The polygon contours to transform.
    /// </param>
    /// <param name="transform">
    /// The transformation matrix to apply.
    /// </param>
    /// <param name="transformerBackend">
    /// The optional transformation backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The transformed polygon contours.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<Vector2>> Transform(
        IReadOnlyList<IReadOnlyList<Vector2>> contours,
        Matrix transform,
        IPolygonTransformer? transformerBackend = null)
    {
        transformerBackend ??= new DefaultPolygonTransformer();

        return transformerBackend.Transform(
            contours,
            transform);
    }

    /// <summary>
    /// Generates offset contours from the specified polygon geometry.
    /// </summary>
    /// <param name="contours">
    /// The polygon contours to offset.
    /// </param>
    /// <param name="options">
    /// The options that control offset generation.
    /// When <see langword="null"/>, default options are used.
    /// </param>
    /// <param name="offsetterBackend">
    /// The optional offset backend to use.
    /// When <see langword="null"/>, the default backend is used.
    /// </param>
    /// <returns>
    /// The generated offset polygon contours.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<Vector2>> Offset(
        IReadOnlyList<IReadOnlyList<Vector2>> contours,
        PolygonOffsetOptions? options = null,
        IPolygonOffsetter? offsetterBackend = null)
    {
        options ??= new PolygonOffsetOptions();
        offsetterBackend ??= new Clipper2PolygonOffsetter();

        return offsetterBackend.Offset(
            contours,
            options);
    }
}