using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Geometry.Internal;
using Sachssoft.Engine.Graphics.Meshes.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Meshes;

/// <summary>
/// Provides methods for creating standard GPU meshes.
/// </summary>
public static partial class MeshGenerator
{
    /// <summary>
    /// Creates a two-dimensional quad on the XY plane using
    /// <see cref="VertexPositionColorTexture"/> vertices.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="size">
    /// The width and height of the generated quad.
    /// </param>
    /// <param name="centerOrigin">
    /// <see langword="true"/> to center the quad around the origin;
    /// otherwise, the quad extends from the origin in the positive
    /// X and Y directions.
    /// </param>
    /// <param name="flipMode">
    /// Specifies how the texture coordinates of the quad are flipped.
    /// </param>
    /// <returns>The generated quad mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateQuad(
        GraphicsDevice graphicsDevice,
        float size = 1f,
        bool centerOrigin = false,
        Texture2DFlipMode flipMode = Texture2DFlipMode.None)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        return new QuadMesh<VertexPositionColorTexture>(
            graphicsDevice,
            static (in MeshVertexData data) => new VertexPositionColorTexture(
                data.Position,
                data.Color,
                data.TextureCoordinate),
            size,
            centerOrigin,
            flipMode);
    }

    /// <summary>
    /// Creates a two-dimensional quad on the XY plane using a custom vertex type.
    /// </summary>
    /// <typeparam name="TVertex">
    /// The GPU vertex type produced for the generated mesh.
    /// </typeparam>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="vertexFactory">
    /// The factory used to convert generated mesh vertex data
    /// into the requested GPU vertex type.
    /// </param>
    /// <param name="size">
    /// The width and height of the generated quad.
    /// </param>
    /// <param name="centerOrigin">
    /// <see langword="true"/> to center the quad around the origin;
    /// otherwise, the quad extends from the origin in the positive
    /// X and Y directions.
    /// </param>
    /// <param name="flipMode">
    /// Specifies how the texture coordinates of the quad are flipped.
    /// </param>
    /// <returns>The generated quad mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or
    /// <paramref name="vertexFactory"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateQuad<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        Texture2DFlipMode flipMode = Texture2DFlipMode.None)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        return new QuadMesh<TVertex>(
            graphicsDevice,
            vertexFactory,
            size,
            centerOrigin,
            flipMode);
    }

    /// <summary>
    /// Creates a planar polygon mesh by triangulating the specified
    /// collection of polygon paths.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="paths">
    /// The polygon paths providing the two-dimensional source geometry.
    /// </param>
    /// <param name="size">
    /// The scale applied to the generated polygon positions.
    /// </param>
    /// <param name="centerOrigin">
    /// <see langword="true"/> to center the polygon bounds around
    /// the origin before applying the scale; otherwise,
    /// <see langword="false"/> to preserve the source origin.
    /// </param>
    /// <param name="triangulatorBackend">
    /// The triangulator used to generate triangle indices from the
    /// polygon paths. If <see langword="null"/>,
    /// <see cref="LibTessPolygonTriangulator"/> is used.
    /// </param>
    /// <returns>
    /// The generated polygon mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or
    /// <paramref name="paths"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreatePolygon(
        GraphicsDevice graphicsDevice,
        IReadOnlyList<IReadOnlyList<Vector2>> paths,
        float size = 1f,
        bool centerOrigin = false,
        IPolygonTriangulator? triangulatorBackend = null)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(paths);

        return CreatePolygon(
            graphicsDevice,
            paths,
            static (in MeshVertexData data) => new VertexPositionColorTexture(
                data.Position,
                data.Color,
                data.TextureCoordinate),
            size,
            centerOrigin,
            triangulatorBackend);
    }

    /// <summary>
    /// Creates a planar polygon mesh by triangulating the specified
    /// collection of polygon paths.
    /// </summary>
    /// <typeparam name="TVertex">
    /// The GPU vertex type produced for the generated mesh.
    /// </typeparam>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="paths">
    /// The polygon paths providing the two-dimensional source geometry.
    /// </param>
    /// <param name="vertexFactory">
    /// The factory used to convert generated mesh vertex data
    /// into the requested GPU vertex type.
    /// </param>
    /// <param name="size">
    /// The scale applied to the generated polygon positions.
    /// </param>
    /// <param name="centerOrigin">
    /// <see langword="true"/> to center the polygon bounds around
    /// the origin before applying the scale; otherwise,
    /// <see langword="false"/> to preserve the source origin.
    /// </param>
    /// <param name="triangulatorBackend">
    /// The triangulator used to generate triangle indices from the
    /// polygon paths. If <see langword="null"/>,
    /// <see cref="LibTessPolygonTriangulator"/> is used.
    /// </param>
    /// <returns>
    /// The generated polygon mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/>,
    /// <paramref name="paths"/>, or <paramref name="vertexFactory"/>
    /// is <see langword="null"/>.
    /// </exception>
    public static IMesh CreatePolygon<TVertex>(
        GraphicsDevice graphicsDevice,
        IReadOnlyList<IReadOnlyList<Vector2>> paths,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        IPolygonTriangulator? triangulatorBackend = null)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        triangulatorBackend ??= new LibTessPolygonTriangulator();

        var result = triangulatorBackend.Triangulate(
            paths,
            new PolygonTriangulationOptions());

        return new PolygonMesh<TVertex>(
            graphicsDevice,
            result.Vertices,
            result.Indices,
            vertexFactory,
            size,
            centerOrigin);
    }

    /// <summary>
    /// Creates a textured cube mesh using
    /// <see cref="VertexPositionTexture"/> vertices.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="size">
    /// The length of each side of the cube.
    /// </param>
    /// <returns>
    /// The generated cube mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateCube(
        GraphicsDevice graphicsDevice,
        float size = 1f)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        return new TextureCubeMesh<VertexPositionTexture>(
            graphicsDevice,
            static (in MeshVertexData data) => new VertexPositionTexture(
                data.Position,
                data.TextureCoordinate),
            size);
    }

    /// <summary>
    /// Creates a cube mesh centered around the origin.
    /// </summary>
    /// <typeparam name="TVertex">
    /// The GPU vertex type produced for the generated mesh.
    /// </typeparam>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="vertexFactory">
    /// The factory used to convert generated mesh vertex data
    /// into the requested GPU vertex type.
    /// </param>
    /// <param name="size">
    /// The length of each side of the cube.
    /// </param>
    /// <returns>
    /// The generated cube mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or
    /// <paramref name="vertexFactory"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateCube<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        return new TextureCubeMesh<TVertex>(
            graphicsDevice,
            vertexFactory,
            size);
    }


    /// <summary>
    /// Creates a textured UV sphere mesh using
    /// <see cref="VertexPositionTexture"/> vertices.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="radius">
    /// The radius of the generated sphere.
    /// </param>
    /// <param name="segments">
    /// The number of longitudinal segments around the sphere.
    /// Must be at least <c>3</c>.
    /// </param>
    /// <param name="rings">
    /// The number of latitudinal rings between the poles.
    /// Must be at least <c>2</c>.
    /// </param>
    /// <returns>The generated sphere mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="segments"/> is less than <c>3</c>,
    /// <paramref name="rings"/> is less than <c>2</c>, or the resulting
    /// vertex count exceeds the supported 16-bit index range.
    /// </exception>
    public static IMesh CreateSphere(
        GraphicsDevice graphicsDevice,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        return new TextureSphereMesh<VertexPositionTexture>(
            graphicsDevice,
            static (in MeshVertexData data) => new VertexPositionTexture(
                data.Position,
                data.TextureCoordinate),
            radius,
            segments,
            rings);
    }

    /// <summary>
    /// Creates a UV sphere mesh centered around the origin.
    /// </summary>
    /// <typeparam name="TVertex">
    /// The GPU vertex type produced for the generated mesh.
    /// </typeparam>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="vertexFactory">
    /// The factory used to convert generated mesh vertex data
    /// into the requested GPU vertex type.
    /// </param>
    /// <param name="radius">
    /// The radius of the generated sphere.
    /// </param>
    /// <param name="segments">
    /// The number of longitudinal segments around the sphere.
    /// Must be at least <c>3</c>.
    /// </param>
    /// <param name="rings">
    /// The number of latitudinal rings between the poles.
    /// Must be at least <c>2</c>.
    /// </param>
    /// <returns>The generated sphere mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or
    /// <paramref name="vertexFactory"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="segments"/> is less than <c>3</c>,
    /// <paramref name="rings"/> is less than <c>2</c>, or the resulting
    /// vertex count exceeds the supported 16-bit index range.
    /// </exception>
    public static IMesh CreateSphere<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        return new TextureSphereMesh<TVertex>(
            graphicsDevice,
            vertexFactory,
            radius,
            segments,
            rings);
    }


    /// <summary>
    /// Creates a skybox cube mesh with inward-facing faces using
    /// <see cref="VertexPositionTexture"/> vertices.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="size">
    /// The length of each side of the skybox cube.
    /// </param>
    /// <returns>
    /// The generated skybox mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateSkybox(
        GraphicsDevice graphicsDevice,
        float size = 1f)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        // Skybox verwendet einen Würfel statt einer Kugel,
        // da eine Kugel an den Polen Texturverzerrungen erzeugt.
        // Der Würfel bietet eine bessere Abbildung für Cubemap-Texturen.

        return new TextureSkyboxMesh<VertexPositionTexture>(
            graphicsDevice,
            static (in MeshVertexData data) => new VertexPositionTexture(
                data.Position,
                data.TextureCoordinate),
            size);
    }

    /// <summary>
    /// Creates a skybox cube mesh with inward-facing faces using
    /// a custom vertex type.
    /// </summary>
    /// <typeparam name="TVertex">
    /// The GPU vertex type produced for the generated mesh.
    /// </typeparam>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the mesh resources.
    /// </param>
    /// <param name="vertexFactory">
    /// The factory used to convert generated mesh vertex data
    /// into the requested GPU vertex type.
    /// </param>
    /// <param name="size">
    /// The length of each side of the skybox cube.
    /// </param>
    /// <returns>
    /// The generated skybox mesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="graphicsDevice"/> or
    /// <paramref name="vertexFactory"/> is <see langword="null"/>.
    /// </exception>
    public static IMesh CreateSkybox<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        return new TextureSkyboxMesh<TVertex>(
            graphicsDevice,
            vertexFactory,
            size);
    }
}