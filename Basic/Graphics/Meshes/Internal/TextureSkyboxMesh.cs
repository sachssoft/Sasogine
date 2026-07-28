using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Meshes.Internal;

internal sealed class TextureSkyboxMesh
    : Mesh<VertexPositionTexture>
{
    public TextureSkyboxMesh(
        GraphicsDevice graphicsDevice,
        float size = 1f)
        : base(
            graphicsDevice,
            CreateVertices(size),
            CreateIndices())
    {
    }


    private static VertexPositionTexture[] CreateVertices(float size)
    {
        float h = size * 0.5f;

        return
        [
            // Front (inside)
            new(new Vector3(-h, -h, -h), new Vector2(0, 1)),
            new(new Vector3( h, -h, -h), new Vector2(1, 1)),
            new(new Vector3( h,  h, -h), new Vector2(1, 0)),
            new(new Vector3(-h,  h, -h), new Vector2(0, 0)),

            // Back
            new(new Vector3( h, -h, h), new Vector2(0, 1)),
            new(new Vector3(-h, -h, h), new Vector2(1, 1)),
            new(new Vector3(-h,  h, h), new Vector2(1, 0)),
            new(new Vector3( h,  h, h), new Vector2(0, 0)),

            // Top
            new(new Vector3(-h, h, -h), new Vector2(0, 1)),
            new(new Vector3( h, h, -h), new Vector2(1, 1)),
            new(new Vector3( h, h, h), new Vector2(1, 0)),
            new(new Vector3(-h, h, h), new Vector2(0, 0)),

            // Bottom
            new(new Vector3(-h, -h, h), new Vector2(0, 1)),
            new(new Vector3( h, -h, h), new Vector2(1, 1)),
            new(new Vector3( h, -h, -h), new Vector2(1, 0)),
            new(new Vector3(-h, -h, -h), new Vector2(0, 0)),

            // Right
            new(new Vector3(h, -h, -h), new Vector2(0, 1)),
            new(new Vector3(h, -h, h), new Vector2(1, 1)),
            new(new Vector3(h, h, h), new Vector2(1, 0)),
            new(new Vector3(h, h, -h), new Vector2(0, 0)),

            // Left
            new(new Vector3(-h, -h, h), new Vector2(0, 1)),
            new(new Vector3(-h, -h, -h), new Vector2(1, 1)),
            new(new Vector3(-h, h, -h), new Vector2(1, 0)),
            new(new Vector3(-h, h, h), new Vector2(0, 0))
        ];
    }


    private static short[] CreateIndices()
    {
        return
        [
            // reversed winding for inside view

            // Front
            0, 2, 1,
            0, 3, 2,

            // Back
            4, 5, 6,
            4, 6, 7,

            // Top
            8, 10, 9,
            8, 11, 10,

            // Bottom
            12, 14, 13,
            12, 15, 14,

            // Right
            16, 18, 17,
            16, 19, 18,

            // Left
            20, 21, 22,
            20, 22, 23
        ];
    }
}