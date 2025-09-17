using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;
using System;
using System.Collections.Generic;
using Sachssoft.Sasogine;

namespace Sachssoft.Graphics.Primitives;

[Obsolete]
public sealed class GridPrimitive : IDisposable
{
    private readonly GraphicsDevice _graphics_device;
    private VertexBuffer? _vertex_buffer;
    private int _line_count;
    private bool _built;
    private Color _color = Color.White;
    private Vector2 _grid_size = new Vector2(10f, 10f);
    private Vector2 _offset = Vector2.Zero;
    private int _columns = 10;
    private int _rows = 10;

    public GridPrimitive()
        : this(IMyGameApp.Game.GraphicsDevice)
    {
    }

    public GridPrimitive(GraphicsDevice graphics_device)
    {
        _graphics_device = graphics_device;
    }

    public void Build()
    {
        var lines = new List<VertexPositionColor>();

        float width = _columns * _grid_size.X;
        float height = _rows * _grid_size.Y;
        Vector3 offset3 = new Vector3(_offset.X, _offset.Y, 0f);

        // vertikale Linien
        for (int x = 0; x <= _columns; x++)
        {
            float xpos = x * _grid_size.X;
            lines.Add(new VertexPositionColor(new Vector3(xpos, 0, 0) + offset3, _color));
            lines.Add(new VertexPositionColor(new Vector3(xpos, height, 0) + offset3, _color));
        }

        // horizontale Linien
        for (int y = 0; y <= _rows; y++)
        {
            float ypos = y * _grid_size.Y;
            lines.Add(new VertexPositionColor(new Vector3(0, ypos, 0) + offset3, _color));
            lines.Add(new VertexPositionColor(new Vector3(width, ypos, 0) + offset3, _color));
        }

        _line_count = lines.Count / 2;

        if (_line_count == 0)
        {
            _vertex_buffer?.Dispose();
            _vertex_buffer = null;
            _built = false;
            return;
        }

        _vertex_buffer?.Dispose();
        _vertex_buffer = new VertexBuffer(_graphics_device, typeof(VertexPositionColor), lines.Count, BufferUsage.WriteOnly);
        _vertex_buffer.SetData(lines.ToArray());

        _built = true;
    }

    public void Draw(IEffectAdapter effect, CameraBase camera, Matrix transform)
    {
        if (!_built)
            Build();

        if (_vertex_buffer == null || _line_count == 0)
            return;

        _graphics_device.SetVertexBuffer(_vertex_buffer);

        // Transformation berücksichtigen
        effect.View = camera.View;
        effect.Projection = camera.Projection;
        effect.World = transform * camera.World;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _graphics_device.DrawPrimitives(
                primitiveType: PrimitiveType.LineList,
                vertexStart: 0,
                primitiveCount: _line_count
            );
        }
    }

    public void Dispose()
    {
        _vertex_buffer?.Dispose();
        _vertex_buffer = null;
        _built = false;
    }

    public Vector2 Offset
    {
        get => _offset;
        set
        {
            if (_offset != value)
            {
                _offset = value;
                _built = false;
            }
        }
    }

    public Vector2 Size
    {
        get => _grid_size;
        set
        {
            if (_grid_size != value)
            {
                _grid_size = value;
                _built = false; // Erzwingt Neuaufbau
            }
        }
    }

    public int Columns
    {
        get => _columns;
        set
        {
            if (_columns != value)
            {
                _columns = value;
                _built = false; // Erzwingt Neuaufbau
            }
        }
    }

    public int Rows
    {
        get => _rows;
        set
        {
            if (_rows != value)
            {
                _rows = value;
                _built = false; // Erzwingt Neuaufbau
            }
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                _built = false; // Neuaufbau nötig wegen Farbänderung
            }
        }
    }
}
