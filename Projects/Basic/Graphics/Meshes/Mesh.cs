using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Meshes;

/// <summary>
/// Represents a GPU mesh containing vertex and index buffers used for rendering.
/// </summary>
/// <typeparam name="TVertex">
/// The vertex type stored in the mesh. Must implement <see cref="IVertexType"/>.
/// </typeparam>
public class Mesh<TVertex> : IMesh
    where TVertex : struct, IVertexType
{
    /// <summary>
    /// Gets the GPU vertex buffer containing the mesh vertex data.
    /// </summary>
    public VertexBuffer VertexBuffer { get; }

    /// <summary>
    /// Gets the GPU index buffer defining the mesh primitives.
    /// </summary>
    public IndexBuffer IndexBuffer { get; }

    /// <summary>
    /// Gets the number of vertices contained in the mesh.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// Gets the number of indices contained in the mesh.
    /// </summary>
    public int IndexCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh{TVertex}"/> class
    /// using 16-bit indices and uploads the vertex and index data to the GPU.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="vertices">
    /// The vertex data of the mesh.
    /// </param>
    /// <param name="indices">
    /// The 16-bit index data of the mesh.
    /// </param>
    public Mesh(
        GraphicsDevice graphicsDevice,
        TVertex[] vertices,
        short[] indices)
    {
        VertexCount = vertices.Length;
        IndexCount = indices.Length;

        VertexBuffer = new VertexBuffer(
            graphicsDevice,
            typeof(TVertex),
            VertexCount,
            BufferUsage.WriteOnly);

        VertexBuffer.SetData(vertices);

        IndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.SixteenBits,
            IndexCount,
            BufferUsage.WriteOnly);

        IndexBuffer.SetData(indices);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh{TVertex}"/> class
    /// using 32-bit indices and uploads the vertex and index data to the GPU.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="vertices">
    /// The vertex data of the mesh.
    /// </param>
    /// <param name="indices">
    /// The 32-bit index data of the mesh.
    /// </param>
    public Mesh(
        GraphicsDevice graphicsDevice,
        TVertex[] vertices,
        int[] indices)
    {
        VertexCount = vertices.Length;
        IndexCount = indices.Length;

        VertexBuffer = new VertexBuffer(
            graphicsDevice,
            typeof(TVertex),
            VertexCount,
            BufferUsage.WriteOnly);

        VertexBuffer.SetData(vertices);

        IndexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            IndexCount,
            BufferUsage.WriteOnly);

        IndexBuffer.SetData(indices);
    }

    /// <summary>
    /// Releases the GPU resources used by this mesh.
    /// </summary>
    public virtual void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }
}