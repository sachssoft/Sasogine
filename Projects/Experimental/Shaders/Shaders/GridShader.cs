using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.VarietyGolf.Graphics.Shaders;

sealed class GridShader : ShaderBase, IShaderTransform
{
    private readonly EffectParameter _projectionParam;
    private readonly EffectParameter _viewParam;
    private readonly EffectParameter _worldParam;
    private readonly EffectParameter _gridSizeParam;
    private readonly EffectParameter _lineWidthParam;
    private readonly EffectParameter _gridColorParam;
    private readonly EffectParameter _rectangleMinParam;
    private readonly EffectParameter _rectangleMaxParam;

    public GridShader(Effect effect)
        : base(effect)
    {
        _projectionParam = effect.Parameters["Projection"];
        _viewParam = effect.Parameters["View"];
        _worldParam = effect.Parameters["World"];
        _gridSizeParam = effect.Parameters["GridSize"];
        _lineWidthParam = effect.Parameters["LineWidth"];
        _gridColorParam = effect.Parameters["GridColor"];
        _rectangleMinParam = effect.Parameters["RectangleMin"];
        _rectangleMaxParam = effect.Parameters["RectangleMax"];
    }


    public float GridSize { get; set; } = 32f;

    /// <summary>
    /// Line width in screen pixels.
    /// </summary>
    public float LineWidth { get; set; } = 1f;

    public Rectangle Rectangle { get; set; }

    public override void Apply()
    {
        if (Camera != null)
        {
            _worldParam.SetValue(Camera.World * Transform);
            _viewParam.SetValue(Camera.View);
            _projectionParam.SetValue(Camera.Projection);
        }
        else
        {
            _worldParam.SetValue(Transform);
        }

        _gridSizeParam.SetValue(GridSize);
        _lineWidthParam.SetValue(LineWidth);
        _gridColorParam.SetValue(Color.ToVector4());
        _rectangleMinParam.SetValue(new Vector2(Rectangle.Left, Rectangle.Top));
        _rectangleMaxParam.SetValue(new Vector2(Rectangle.Right, Rectangle.Bottom));

        base.Apply();
    }


    public override ShaderBase Clone()
    {
        return new GridShader(Effect.Clone())
        {
            GraphicsDevice = GraphicsDevice,
            Camera = Camera,
            Transform = Transform,
            GridSize = GridSize,
            LineWidth = LineWidth,
            Color = Color,
            Rectangle = Rectangle
        };
    }
}