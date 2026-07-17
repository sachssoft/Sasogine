using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Primitives;

public sealed class PrimitiveBatch : IDisposable
{
    private readonly List<PrimitiveBase> _primitives = new();

    private VertexPositionColorNormalTexture[]? _vertexArray;
    private short[]? _indexArray;

    private DynamicVertexBuffer? _vertexBuffer;
    private DynamicIndexBuffer? _indexBuffer;

    private bool _dirty = true;

    public PrimitiveBatch()
    {
    }

    public void Add(PrimitiveBase primitive)
    {
        if (primitive == null) throw new ArgumentNullException(nameof(primitive));
        _primitives.Add(primitive);
        _dirty = true;
    }

    public void Remove(PrimitiveBase primitive)
    {
        if (_primitives.Remove(primitive))
            _dirty = true;
    }

    public void Clear()
    {
        _primitives.Clear();
        _dirty = true;
    }

    public void Draw(SceneDrawContext context, Matrix? transform = null, ICamera? camera = null, IShader? customEffectAdapter = null)
    {
        if (_primitives.Count == 0) return;

        var effectAdapter = customEffectAdapter ?? context.Shader;
        camera ??= context.ViewCamera ?? throw new InvalidOperationException("No camera available.");
        var graphics = context.GraphicsDevice;

        // Berechne Gesamtgrößen
        int totalVertices = 0;
        int totalIndices = 0;
        foreach (var p in _primitives)
        {
            if (!p.Visible) continue;
            totalVertices += p.VertexCount;
            totalIndices += p.IndexCount;
        }

        if (totalVertices == 0 || totalIndices == 0) return;

        _vertexArray ??= new VertexPositionColorNormalTexture[totalVertices];
        _indexArray ??= new short[totalIndices];

        // Fill nur wenn dirty
        if (_dirty)
        {
            int vOffset = 0, iOffset = 0;
            foreach (var p in _primitives)
            {
                if (!p.Visible) continue;
                if (p.FillVisible || p.OutlineVisible)
                    p.Fill(_vertexArray, vOffset, _indexArray, iOffset, (short)vOffset);
                vOffset += p.VertexCount;
                iOffset += p.IndexCount;
            }
            _dirty = false;
        }

        // GPU Buffers
        if (_vertexBuffer == null || _vertexBuffer.VertexCount < totalVertices)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), totalVertices, BufferUsage.WriteOnly);
        }

        if (_indexBuffer == null || _indexBuffer.IndexCount < totalIndices)
        {
            _indexBuffer?.Dispose();
            _indexBuffer = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, totalIndices, BufferUsage.WriteOnly);
        }

        _vertexBuffer.SetData(_vertexArray, 0, totalVertices, SetDataOptions.Discard);
        _indexBuffer.SetData(_indexArray, 0, totalIndices, SetDataOptions.Discard);

        // Matrizen setzen
        if (effectAdapter is IShaderTransform shaderTransform)
        {
            shaderTransform.Camera = camera;
            shaderTransform.Transform = transform ?? Matrix.Identity;
        }
        //effectAdapter.World = transform ?? Matrix.Identity * camera.World;
        //effectAdapter.View = camera.View;
        //effectAdapter.Projection = camera.Projection;
        effectAdapter.Apply();

        foreach (var pass in effectAdapter.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.Indices = _indexBuffer;

            // DrawFill
            graphics.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertexArray,
                0,
                totalVertices,
                _indexArray,
                0,
                totalIndices / 3
            );

            // DrawOutline separat
            foreach (var p in _primitives)
            {
                if (p.OutlineVisible)
                {
                    graphics.DrawUserPrimitives(
                        PrimitiveType.LineStrip,
                        _vertexArray,
                        0,
                        p.VertexCount - 1
                    );
                }
            }
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}
