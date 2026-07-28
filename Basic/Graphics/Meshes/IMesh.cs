using System;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Meshes;

/// <summary>
/// Represents a GPU mesh containing vertex and index buffers used for rendering.
/// </summary>
public interface IMesh : IDisposable
{
    /// <summary>
    /// Gets the vertex buffer containing the mesh vertex data.
    /// </summary>
    VertexBuffer VertexBuffer { get; }

    /// <summary>
    /// Gets the index buffer defining the mesh primitives.
    /// </summary>
    IndexBuffer IndexBuffer { get; }

    /// <summary>
    /// Gets the number of vertices contained in the mesh.
    /// </summary>
    int VertexCount { get; }

    /// <summary>
    /// Gets the number of indices contained in the mesh.
    /// </summary>
    int IndexCount { get; }
}