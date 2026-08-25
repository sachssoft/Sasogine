using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Meshes.Internal
{
    internal sealed class PolygonMesh : Mesh<VertexPositionColorTexture>
    {
        public PolygonMesh(
            GraphicsDevice graphicsDevice,
            IReadOnlyList<Vector2> positions,
            IReadOnlyList<int> indices,
            float size,
            bool centerOrigin)
            : base(
                graphicsDevice,
                CreateVertices(positions, size, centerOrigin),
                CreateIndices(indices))
        {
        }

        private static VertexPositionColorTexture[] CreateVertices(
            IReadOnlyList<Vector2> positions,
            float size,
            bool centerOrigin)
        {
            var offset = centerOrigin
                ? GetBoundsCenter(positions)
                : Vector2.Zero;

            var vertices = new VertexPositionColorTexture[positions.Count];

            for (var i = 0; i < positions.Count; i++)
            {
                var position = positions[i] - offset;

                vertices[i] = new VertexPositionColorTexture(
                    new Vector3(position * size, 0f),
                    Color.White,
                    position);
            }

            return vertices;
        }

        private static short[] CreateIndices(
            IReadOnlyList<int> indices)
        {
            if (indices.Count > short.MaxValue)
                throw new ArgumentException(
                    "The polygon contains too many indices for a 16-bit index buffer.",
                    nameof(indices));

            var result = new short[indices.Count];

            for (var i = 0; i < indices.Count; i++)
                result[i] = checked((short)indices[i]);

            return result;
        }

        private static Vector2 GetBoundsCenter(
            IReadOnlyList<Vector2> positions)
        {
            if (positions.Count == 0)
                return Vector2.Zero;

            var min = positions[0];
            var max = positions[0];

            for (var i = 1; i < positions.Count; i++)
            {
                min = Vector2.Min(min, positions[i]);
                max = Vector2.Max(max, positions[i]);
            }

            return (min + max) * 0.5f;
        }
    }
}