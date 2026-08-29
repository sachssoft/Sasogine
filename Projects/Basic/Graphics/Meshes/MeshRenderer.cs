using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Materials;
using Sachssoft.Sasogine.Graphics.Meshes;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Renders GPU meshes.
/// </summary>
public static class MeshRenderer
{
    /// <summary>
    /// Draws a mesh using the default material and optional camera and transform settings
    /// from the specified drawing context.
    /// </summary>
    /// <param name="context">The scene drawing context used for rendering.</param>
    /// <param name="mesh">The mesh to draw.</param>
    /// <param name="shader">
    /// The shader used to render the mesh. If <see langword="null"/>, the shader
    /// from the default material is used.
    /// </param>
    /// <param name="camera">
    /// The camera transform used for rendering. If <see langword="null"/>, the
    /// view camera from the drawing context is used.
    /// </param>
    /// <param name="transform">
    /// The world transformation applied to the mesh.
    /// </param>
    /// <param name="primitiveType">
    /// The primitive type used to interpret the mesh indices.
    /// </param>
    public static void Draw(
        SceneDrawContext context,
        IMesh mesh,
        IShader? shader = null,
        ICameraTransform? camera = null,
        Matrix? transform = null,
        PrimitiveType primitiveType = PrimitiveType.TriangleList)
    {
        Draw(context.GraphicsDevice,
             mesh,
             shader ?? context.DefaultMaterial.Shader,
             camera ?? context.ViewCamera,
             transform,
             primitiveType);
    }

    /// <summary>
    /// Draws a mesh using the specified shader.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="mesh">The mesh to draw.</param>
    /// <param name="shader">The shader used to render the mesh.</param>
    /// <param name="camera">
    /// The camera transform used for rendering.
    /// </param>
    /// <param name="transform">
    /// The world transformation applied to the mesh.
    /// </param>
    /// <param name="primitiveType">
    /// The primitive type used to interpret the mesh indices.
    /// </param>
    public static void Draw(
        GraphicsDevice graphicsDevice,
        IMesh mesh,
        IShader shader,
        ICameraTransform? camera = null,
        Matrix? transform = null,
        PrimitiveType primitiveType = PrimitiveType.TriangleList)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(shader);

        graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
        graphicsDevice.Indices = mesh.IndexBuffer;

        if (shader is IShaderTransform shaderTransform)
        {
            shaderTransform.Camera = camera;
            shaderTransform.Transform = transform ?? Matrix.Identity;
        }

        shader.Apply();

        int primitiveCount = GetPrimitiveCount(
            primitiveType,
            mesh.IndexCount);

        foreach (EffectPass pass in shader.CurrentTechnique.Passes)
        {
            pass.Apply();

            graphicsDevice.DrawIndexedPrimitives(
                primitiveType,
                0,
                0,
                primitiveCount);
        }
    }

    private static int GetPrimitiveCount(
        PrimitiveType primitiveType,
        int indexCount)
    {
        return primitiveType switch
        {
            PrimitiveType.TriangleList => indexCount / 3,
            PrimitiveType.TriangleStrip => indexCount - 2,
            PrimitiveType.LineList => indexCount / 2,
            PrimitiveType.LineStrip => indexCount - 1,
            PrimitiveType.PointList => indexCount,
            _ => throw new NotSupportedException(
                $"Primitive type '{primitiveType}' is not supported.")
        };
    }
}