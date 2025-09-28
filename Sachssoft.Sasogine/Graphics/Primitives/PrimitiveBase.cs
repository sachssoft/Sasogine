using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;

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

    public abstract int VertexCount { get; }
    public abstract int IndexCount { get; }

    public Action<IEffectAdapter, CameraBase, Matrix?>? EffectSetupCallback { get; set; }

    // Methods
    public void MarkDirty() => _dirty = true;

    public abstract void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex);

    protected (float u0, float v0, float u1, float v1) GetUV(Point textureSize)
    {
        if (textureSize.X == 0 || textureSize.Y == 0 || !SourceRect.HasValue)
            return (0f, 0f, 1f, 1f);

        var src = SourceRect.Value;
        return ((float)src.X / textureSize.X,
                (float)src.Y / textureSize.Y,
                (float)(src.X + src.Width) / textureSize.X,
                (float)(src.Y + src.Height) / textureSize.Y);
    }

    public virtual void Update(GameFrameContext context) { }

    public void DrawScoped(GameFrameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null, RenderOptions? options = null)
    {
        using (new RenderScope(context, options))
        {
            Draw(context, transform, customCamera, customEffect);
        }
    }

    public void Draw(GameFrameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null)
    {
        if (!Visible || VertexCount == 0 || IndexCount == 0)
            return;

        var graphics = context.GraphicsDevice;
        var effect = customEffect ?? context.Runtime.Effect;
        var camera = customCamera ?? context.Runtime.Camera ?? throw new InvalidOperationException("No camera available.");

        // Persistente Arrays
        _vertexBuffer ??= new VertexPositionColorNormalTexture[VertexCount];
        _indexBuffer ??= new short[IndexCount];

        if (_dirty)
        {
            Fill(_vertexBuffer, 0, _indexBuffer, 0, 0);
            _dirty = false;
        }

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

        // Matrizen setzen
        effect.World = (transform ?? Matrix.Identity) * camera.World;
        effect.View = camera.View;
        effect.Projection = camera.Projection;
        effect.Apply();

        if (EffectSetupCallback != null)
            EffectSetupCallback.Invoke(effect, camera, transform);
        else
            EffectSetup(effect, camera, transform);

        // DrawFill
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

        // DrawOutline
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

    protected virtual void EffectSetup(IEffectAdapter effect, CameraBase camera, Matrix? transform)
    {
        // Unterklassen können überschreiben
    }


}
