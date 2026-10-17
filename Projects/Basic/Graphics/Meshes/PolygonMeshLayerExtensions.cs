using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Geometry.Internal;
using System;

namespace Sachssoft.Engine.Graphics.Meshes;

/// <summary>
/// Ergänzt bestehende GPU-Meshes und die Polygon-Mesh-Erzeugung um optionale Layer.
/// </summary>
public static class PolygonMeshLayerExtensions
{
    /// <summary>
    /// Übernimmt ein bestehendes Mesh als Basis einer optionalen Mesh-Komposition.
    /// </summary>
    /// <remarks>
    /// Es wird kein neues Basis-Mesh erzeugt; das übergebene Mesh wird übernommen.
    /// </remarks>
    public static LayeredPolygonMesh WithLayers(this IMesh baseMesh) =>
        new(baseMesh);

    /// <summary>
    /// Trianguliert eine zusätzliche Path-Geometrie und registriert sie als Layer.
    /// </summary>
    public static PolygonMeshLayer AddPolygonLayer(
        this LayeredPolygonMesh owner,
        string name,
        GraphicsDevice graphicsDevice,
        Path path,
        float size = 1f,
        bool centerOrigin = false,
        IPolygonTriangulator? triangulatorBackend = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(path);

        if (owner.ContainsLayer(name))
            throw new ArgumentException($"A layer named '{name}' already exists.", nameof(name));

        IMesh mesh = MeshGenerator.CreatePolygon(
            graphicsDevice,
            path,
            size,
            centerOrigin,
            triangulatorBackend);

        return owner.AddLayer(name, mesh);
    }

    /// <summary>
    /// Trianguliert eine zusätzliche Path-Geometrie mit einem eigenen Vertex-Typ.
    /// </summary>
    public static PolygonMeshLayer AddPolygonLayer<TVertex>(
        this LayeredPolygonMesh owner,
        string name,
        GraphicsDevice graphicsDevice,
        Path path,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        IPolygonTriangulator? triangulatorBackend = null)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        if (owner.ContainsLayer(name))
            throw new ArgumentException($"A layer named '{name}' already exists.", nameof(name));

        IMesh mesh = MeshGenerator.CreatePolygon(
            graphicsDevice,
            path,
            vertexFactory,
            size,
            centerOrigin,
            triangulatorBackend);

        return owner.AddLayer(name, mesh);
    }
}
