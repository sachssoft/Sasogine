using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Meshes.Internal;

internal sealed class QuadMesh
    : Mesh<VertexPositionColorTexture>
{
    public QuadMesh(
        GraphicsDevice graphicsDevice,
        float size = 1f,
        bool centerOrigin = false)
        : base(
            graphicsDevice,
            CreateVertices(size, centerOrigin),
            CreateIndices())
    {
    }

    private static VertexPositionColorTexture[] CreateVertices(
        float size,
        bool centerOrigin)
    {
        float offset = centerOrigin ? size * 0.5f : 0f;

        // Top-Left-basierte Koordinaten sind für 2D-Rendering einfacher:
        // (0,0) entspricht der linken oberen Ecke.
        // Bei centerOrigin wird das Quad zusätzlich um den Mittelpunkt verschoben.
        return
        [
            new VertexPositionColorTexture(
                new Vector3(-offset, -offset, 0f),
                Color.White,
                new Vector2(0f, 1f)),

            new VertexPositionColorTexture(
                new Vector3(size - offset, -offset, 0f),
                Color.White,
                new Vector2(1f, 1f)),

            new VertexPositionColorTexture(
                new Vector3(size - offset, size - offset, 0f),
                Color.White,
                new Vector2(1f, 0f)),

            new VertexPositionColorTexture(
                new Vector3(-offset, size - offset, 0f),
                Color.White,
                new Vector2(0f, 0f))
        ];
    }

    private static short[] CreateIndices()
    {
        return
        [
            0, 1, 2,
            0, 2, 3
        ];
    }
}