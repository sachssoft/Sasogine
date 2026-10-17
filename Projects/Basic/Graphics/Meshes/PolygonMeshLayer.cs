using System;

namespace Sachssoft.Engine.Graphics.Meshes;

/// <summary>
/// Beschreibt ein zusätzliches, unabhängig renderbares Mesh-Layer.
/// </summary>
/// <remarks>
/// Das zugehörige Mesh gehört dem übergeordneten <see cref="LayeredPolygonMesh"/>.
/// Die Geometrie und der Vertex-Typ können sich vom Basis-Mesh unterscheiden.
/// </remarks>
public sealed class PolygonMeshLayer
{
    internal PolygonMeshLayer(string name, IMesh mesh)
    {
        Name = name;
        Mesh = mesh;
    }

    /// <summary>
    /// Ruft den eindeutigen Namen des Layers ab.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Ruft das eigenständige GPU-Mesh des Layers ab.
    /// </summary>
    public IMesh Mesh { get; }

    /// <summary>
    /// Gibt an, ob das Layer von aufrufendem Rendering-Code gezeichnet werden soll.
    /// </summary>
    /// <remarks>
    /// Die Klasse zeichnet nicht automatisch und ändert keine Renderzustände.
    /// </remarks>
    public bool Enabled { get; set; } = true;
}
