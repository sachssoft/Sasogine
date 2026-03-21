using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Tile;

public class TileBatch : IDisposable
{
    private readonly GraphicsDevice _device;
    private readonly List<TileVertex> _vertices = new();
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private readonly Effect _effect;
    private Texture2D _currentTexture;
    private bool _begun;

    public TileBatch(GraphicsDevice device, Effect effect)
    {
        _device = device;
        _effect = effect;

        _vertexBuffer = new VertexBuffer(device, TileVertex.VertexDeclaration, 1024, BufferUsage.WriteOnly);
        _indexBuffer = CreateQuadIndexBuffer(device, 1024);
    }

    public void Begin(Texture2D texture)
    {
        _vertices.Clear();
        _currentTexture = texture;
        _begun = true;
    }

    public void DrawTile(Matrix transform, Color color, Rectangle? source = null)
    {
        if (!_begun)
            throw new InvalidOperationException("Begin must be called before DrawTile");

        Vector2 texSize = new(_currentTexture.Width, _currentTexture.Height);

        Rectangle src = source ?? new Rectangle(0, 0, _currentTexture.Width, _currentTexture.Height);

        Vector2 uvTL = new(src.X / texSize.X, src.Y / texSize.Y);
        Vector2 uvBR = new((src.X + src.Width) / texSize.X, (src.Y + src.Height) / texSize.Y);

        float w = src.Width;
        float h = src.Height;

        Vector3 p0 = Vector3.Transform(new Vector3(0, h, 0), transform);  // Bottom-left
        Vector3 p1 = Vector3.Transform(new Vector3(w, h, 0), transform);  // Bottom-right
        Vector3 p2 = Vector3.Transform(new Vector3(w, 0, 0), transform);  // Top-right
        Vector3 p3 = Vector3.Transform(new Vector3(0, 0, 0), transform);  // Top-left

        _vertices.Add(new TileVertex { Position = p0, TexCoord = new Vector2(uvTL.X, uvBR.Y), Color = color });
        _vertices.Add(new TileVertex { Position = p1, TexCoord = new Vector2(uvBR.X, uvBR.Y), Color = color });
        _vertices.Add(new TileVertex { Position = p2, TexCoord = new Vector2(uvBR.X, uvTL.Y), Color = color });
        _vertices.Add(new TileVertex { Position = p3, TexCoord = new Vector2(uvTL.X, uvTL.Y), Color = color });
    }


    public void End(Matrix world, Matrix view, Matrix projection)
    {
        if (!_begun) return;
        _begun = false;

        if (_vertices.Count == 0)
            return;

        _vertexBuffer.SetData(_vertices.ToArray());
        _device.SetVertexBuffer(_vertexBuffer);
        _device.Indices = _indexBuffer;

        // Kombinierte Matrix vorberechnen
        var wvp = world * view * projection;

        // Effekt-Parameter setzen
        if (_effect is TileEffect tileEffect)
        {
            tileEffect.WorldViewProjection = wvp;
            tileEffect.Texture = _currentTexture;
        }
        else
        {
            _effect.Parameters["WorldViewProjection"]?.SetValue(wvp);
            _effect.Parameters["Texture"]?.SetValue(_currentTexture);
        }

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.Count / 2);
        }
    }


    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }

    private static IndexBuffer CreateQuadIndexBuffer(GraphicsDevice device, int maxQuads)
    {
        var indices = new short[maxQuads * 6];
        for (int i = 0; i < maxQuads; i++)
        {
            short baseIndex = (short)(i * 4);
            indices[i * 6 + 0] = baseIndex;
            indices[i * 6 + 1] = (short)(baseIndex + 1);
            indices[i * 6 + 2] = (short)(baseIndex + 2);
            indices[i * 6 + 3] = baseIndex;
            indices[i * 6 + 4] = (short)(baseIndex + 2);
            indices[i * 6 + 5] = (short)(baseIndex + 3);
        }

        var indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
        return indexBuffer;
    }
}
