using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class QuadMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public QuadMesh(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        Texture2DFlipMode flipMode = Texture2DFlipMode.None,
        int horizontalSegments = 0,
        int verticalSegments = 0)
        : base(
            graphicsDevice,
            CreateVertices(size, centerOrigin, flipMode, horizontalSegments, verticalSegments, vertexFactory),
            CreateIndices(horizontalSegments, verticalSegments))
    {
    }

    private static TVertex[] CreateVertices(
        float size,
        bool centerOrigin,
        Texture2DFlipMode flipMode,
        int horizontalSegments,
        int verticalSegments,
        MeshVertexFactory<TVertex> vertexFactory)
    {
        float offset = centerOrigin ? size * 0.5f : 0f;

        bool flipHorizontal = (flipMode & Texture2DFlipMode.Horizontal) != 0;
        bool flipVertical = (flipMode & Texture2DFlipMode.Vertical) != 0;

        float left = flipHorizontal ? 1f : 0f;
        float right = flipHorizontal ? 0f : 1f;
        float top = flipVertical ? 0f : 1f;
        float bottom = flipVertical ? 1f : 0f;

        var normal = Vector3.UnitZ;
        var tangent = Vector3.UnitX;
        var bitangent = Vector3.UnitY;

        int columns = horizontalSegments + 2;
        int rows = verticalSegments + 2;
        var vertices = new TVertex[columns * rows];

        for (int xIndex = 0; xIndex < columns; xIndex++)
        {
            float xAmount = xIndex / (float)(columns - 1);
            float x = size * xAmount - offset;
            float u = MathHelper.Lerp(left, right, xAmount);

            for (int yIndex = 0; yIndex < rows; yIndex++)
            {
                float yAmount = yIndex / (float)(rows - 1);
                float y = size * yAmount - offset;
                float v = MathHelper.Lerp(top, bottom, yAmount);

                vertices[xIndex * rows + yIndex] = vertexFactory(new MeshVertexData(
                    new Vector3(x, y, 0f),
                    normal,
                    tangent,
                    bitangent,
                    Color.White,
                    new Vector2(u, v)));
            }
        }

        return vertices;
    }

    private static int[] CreateIndices(int horizontalSegments, int verticalSegments)
    {
        int columns = horizontalSegments + 2;
        int rows = verticalSegments + 2;
        int quadColumns = columns - 1;
        int quadRows = rows - 1;
        var indices = new int[quadColumns * quadRows * 6];
        int index = 0;

        for (int xIndex = 0; xIndex < quadColumns; xIndex++)
        {
            for (int yIndex = 0; yIndex < quadRows; yIndex++)
            {
                int topLeft = xIndex * rows + yIndex;
                int topRight = (xIndex + 1) * rows + yIndex;
                int bottomRight = topRight + 1;
                int bottomLeft = topLeft + 1;

                indices[index++] = topLeft;
                indices[index++] = topRight;
                indices[index++] = bottomRight;
                indices[index++] = topLeft;
                indices[index++] = bottomRight;
                indices[index++] = bottomLeft;
            }
        }

        return indices;
    }
}
