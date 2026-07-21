using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Primitives;

[Obsolete("Remove soon")]
public abstract class PrimitiveBase
{
    // Fields
    private Color _fillColor = Color.White;
    private Color _outlineColor = Color.Black;
    private Rectangle? _sourceRect;
    private Point _textureSize = new Point(1);

    private VertexPositionColorNormalTexture[]? _vertexBuffer;
    private short[]? _indexBuffer;

    private DynamicVertexBuffer? _dynamicVertexBuffer;
    private DynamicIndexBuffer? _dynamicIndexBuffer;

    private bool _dirty = true;
    private FlipMode _uvFlipMode;

    // Constructor
    protected PrimitiveBase() { }

    // Properties
    public Color FillColor
    {
        get => _fillColor;
        set { if (_fillColor != value) { _fillColor = value; MarkDirty(); } }
    }

    public Color OutlineColor
    {
        get => _outlineColor;
        set { if (_outlineColor != value) { _outlineColor = value; MarkDirty(); } }
    }

    public bool OutlineVisible { get; set; } = false; // immer false
    public bool FillVisible { get; set; } = true;     // immer true
    public bool Visible { get; set; } = true;

    public Rectangle? SourceRect
    {
        get => _sourceRect;
        set { if (_sourceRect != value) { _sourceRect = value; MarkDirty(); } }
    }

    public Point TextureSize
    {
        get => _textureSize;
        set { if (_textureSize != value) { _textureSize = value; MarkDirty(); } }
    }

    public FlipMode UVFlipMode
    {
        get => _uvFlipMode;
        set { if (_uvFlipMode != value) { _uvFlipMode = value; MarkDirty(); } }
    }

    public abstract int VertexCount { get; }
    public abstract int IndexCount { get; }

    public Action<IShader, ICamera, Matrix?>? EffectSetupCallback { get; set; }

    // Methods
    public void MarkDirty() => _dirty = true;

    public abstract void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex);

    protected (float u0, float v0, float u1, float v1) GetUV(Point textureSize)
    {
        // Fallback für leere Textur
        if (textureSize.X == 0 || textureSize.Y == 0)
            return (0f, 0f, 1f, 1f);

        float u0 = 0f;
        float v0 = 0f;
        float u1 = 1f;
        float v1 = 1f;

        if (SourceRect.HasValue)
        {
            var src = SourceRect.Value;
            u0 = (float)src.X / textureSize.X;
            v0 = (float)src.Y / textureSize.Y;
            u1 = (float)(src.X + src.Width) / textureSize.X;
            v1 = (float)(src.Y + src.Height) / textureSize.Y;
        }

        // Flip anwenden
        if (UVFlipMode.HasFlag(FlipMode.Horizontal))
        {
            var temp = u0; u0 = u1; u1 = temp;
        }
        if (UVFlipMode.HasFlag(FlipMode.Vertical))
        {
            var temp = v0; v0 = v1; v1 = temp;
        }

        return (u0, v0, u1, v1);
    }

    public virtual void Update(SceneUpdateContext context) { }

    public void DrawScoped(SceneDrawContext context, Matrix? transform = null, ICamera? customCamera = null, IShader? customEffect = null, RenderOptions? options = null)
    {
        using (new RenderScope(context, options))
        {
            Draw(context, transform, customCamera, customEffect);
        }
    }

    private void DrawDebugWireframeUnique(GraphicsDevice graphics)
    {
        if (_vertexBuffer == null || _indexBuffer == null)
            return;

        var drawnEdges = new HashSet<(short, short)>();

        for (int i = 0; i < _indexBuffer.Length; i += 3)
        {
            var edges = new (short, short)[]
            {
            (_indexBuffer[i], _indexBuffer[i + 1]),
            (_indexBuffer[i + 1], _indexBuffer[i + 2]),
            (_indexBuffer[i + 2], _indexBuffer[i])
            };

            foreach (var (a, b) in edges)
            {
                // Kanten normalisieren (kleiner Index zuerst)
                var edge = a < b ? (a, b) : (b, a);
                if (drawnEdges.Add(edge))
                {
                    var lineVertices = new VertexPositionColorNormalTexture[]
                    {
                    _vertexBuffer[edge.Item1],
                    _vertexBuffer[edge.Item2]
                    };
                    graphics.DrawUserPrimitives(
                        PrimitiveType.LineList,
                        lineVertices,
                        0,
                        1
                    );
                }
            }
        }
    }

    public void Draw(
        SceneDrawContext context, 
        Matrix? transform = null, 
        ICamera? customCamera = null, 
        IShader? customEffectAdapter = null)
    {
        if (!Visible)
            return;

        var graphics = context.GraphicsDevice;
        var effect = customEffectAdapter ?? context.DefaultMaterial.Shader;
        var camera = customCamera ?? context.ViewCamera ?? throw new InvalidOperationException("No camera available.");

        if (effect.Effect.IsDisposed)
            throw new InvalidOperationException();

        _vertexBuffer ??= new VertexPositionColorNormalTexture[VertexCount];
        _indexBuffer ??= new short[IndexCount];

        // ✅ Fill zuerst, wenn dirty
        if (_dirty)
        {
            Fill(_vertexBuffer, 0, _indexBuffer, 0, 0);
            _dirty = false;
        }

        // Jetzt prüfen: Wenn immer noch keine Vertices/Indices -> Draw abbrechen
        if (VertexCount == 0 || IndexCount == 0)
            return;

        // Buffers updaten (wie gehabt)
        if (_dynamicVertexBuffer == null || _dynamicVertexBuffer.VertexCount < VertexCount)
        {
            _dynamicVertexBuffer?.Dispose();
            _dynamicVertexBuffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), VertexCount, BufferUsage.WriteOnly);
        }

        if (_dynamicIndexBuffer == null || _dynamicIndexBuffer.IndexCount < IndexCount)
        {
            _dynamicIndexBuffer?.Dispose();
            _dynamicIndexBuffer = new DynamicIndexBuffer(graphics, IndexElementSize.SixteenBits, IndexCount, BufferUsage.WriteOnly);
        }

        _dynamicVertexBuffer.SetData(_vertexBuffer, 0, VertexCount, SetDataOptions.Discard);
        _dynamicIndexBuffer.SetData(_indexBuffer, 0, IndexCount, SetDataOptions.Discard);

        //// Matrizen setzen
        //effect.World = (transform ?? Matrix.Identity) * camera.World;
        //effect.View = camera.View;
        //effect.Projection = camera.Projection;
        //effect.Apply();

        //EffectSetupCallback?.Invoke(effect, camera, transform);
        //EffectSetup(effect, camera, transform);

        // Matrizen setzen
        //effect.World = (transform ?? Matrix.Identity) * camera.World;
        //effect.View = camera.View;
        //effect.Projection = camera.Projection;
        if (effect is IShaderTransform shaderTransform)
        {
            shaderTransform.Camera = camera;
            shaderTransform.Transform = transform ?? Matrix.Identity;
        }

        EffectSetupCallback?.Invoke(effect, camera, transform);
        EffectSetup(effect, camera, transform);

        effect.Apply();

        // Fill zeichnen
        if (FillVisible)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.SetVertexBuffer(_dynamicVertexBuffer);
                graphics.Indices = _dynamicIndexBuffer;
                graphics.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexBuffer,
                    0,
                    VertexCount,
                    _indexBuffer,
                    0,
                    IndexCount / 3
                );
            }
        }

        // Optional Outline
        if (OutlineVisible)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.SetVertexBuffer(_dynamicVertexBuffer);
                graphics.DrawUserPrimitives(
                    PrimitiveType.LineStrip,
                    _vertexBuffer,
                    0,
                    VertexCount - 1
                );
            }
        }
    }

    protected virtual void EffectSetup(IShader effect, ICamera camera, Matrix? transform)
    {
        // Unterklassen können überschreiben
    }


}
