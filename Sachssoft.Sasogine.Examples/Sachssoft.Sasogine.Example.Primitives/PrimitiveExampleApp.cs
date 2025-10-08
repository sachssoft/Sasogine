using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision.Shapes;
using Sachssoft.Graphics.Primitives;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Graphics.Primitives;
using Sachssoft.Sasogine.Interactions;

namespace Sachssoft.Sasogine.Example.Primitives
{
    public class PrimitiveExampleApp : MyGameApp<PrimitiveExampleAssets>
    {
        private KeyboardInteractionManager _keyInteraction = new();
        private MouseInteractionManager _mouseInteraction = new();
        private SpriteBatch? _spriteBatch;
        private BasicEffectAdapter? _effect;
        private CameraBase? _camera;

        private PrimitiveView _view;
        private QuadPrimitive? _rectanglePrimitive;
        private EllipsePrimitive? _ellipsePrimitive;
        private PolygonPrimitive? _polygonPrimitive;
        private ShapePrimitive? _shapePrimitive;
        private DicePrimitive? _dicePrimitive;
        private SpherePrimitive? _spherePrimitive;

        private ShapePrimitive _parsedShapePrimitive;

        protected override PrimitiveExampleAssets CreateAssetManager()
        {
            return new PrimitiveExampleAssets(this);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Key Bindings
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F1, () => SwitchView(PrimitiveView.Rectangle));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F2, () => SwitchView(PrimitiveView.Ellipse));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F3, () => SwitchView(PrimitiveView.Polygon));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F4, () => SwitchView(PrimitiveView.Custom));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F5, () => SwitchView(PrimitiveView.Parser));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F6, () => SwitchView(PrimitiveView.Dice));
            _keyInteraction.Add(Microsoft.Xna.Framework.Input.Keys.F7, () => SwitchView(PrimitiveView.Sphere));

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _effect = new BasicEffectAdapter(GraphicsDevice);

            // Default: 2D Camera
            _camera = new Camera2D(GraphicsDevice);

            // Create primitives
            _rectanglePrimitive = new QuadPrimitive();
            _ellipsePrimitive = new EllipsePrimitive();

            var polygonPath = PolygonHelper.RegularPolygon(Vector2.Zero, 1f, 6);
            _polygonPrimitive = new PolygonPrimitive(polygonPath);
            _shapePrimitive = ShapePrimitive.Create(polygonPath);

            _dicePrimitive = new DicePrimitive();
            _spherePrimitive = new SpherePrimitive();

            _parsedShapePrimitive = ShapePrimitive.Create(PathParser.Parse("M18,18.5A1.5,1.5 0 0,1 16.5,17A1.5,1.5 0 0,1 18,15.5A1.5,1.5 0 0,1 19.5,17A1.5,1.5 0 0,1 18,18.5M19.5,9.5L21.46,12H17V9.5M6,18.5A1.5,1.5 0 0,1 4.5,17A1.5,1.5 0 0,1 6,15.5A1.5,1.5 0 0,1 7.5,17A1.5,1.5 0 0,1 6,18.5M20,8H17V4H3C1.89,4 1,4.89 1,6V17H3A3,3 0 0,0 6,20A3,3 0 0,0 9,17H15A3,3 0 0,0 18,20A3,3 0 0,0 21,17H23V12L20,8Z"));
        }

        private void SwitchView(PrimitiveView newView)
        {
            _view = newView;

            // Automatischer Kamerawechsel
            if (newView == PrimitiveView.Dice || newView == PrimitiveView.Sphere)
            {
                _camera = new Camera3D(GraphicsDevice);
            }
            else
            {
                _camera = new Camera2D(GraphicsDevice);
            }
        }

        protected override void OnUpdate(GameFrameContext context)
        {
            base.OnUpdate(context);

            _keyInteraction.Update(context);
            _camera?.Update(context);
        }

        protected override void OnDraw(GameFrameContext context)
        {
            base.OnDraw(context);

            if (_spriteBatch == null || _effect == null || _camera == null)
                return;

            var gd = context.GraphicsDevice;
            gd.Clear(Color.DarkSlateBlue);

            var font = Assets.GetFont(24);

            // Info Text
            _spriteBatch.Begin();
            string text =
$@"Press F1–F6 to switch primitive
F1: Rectangle
F2: Ellipse
F3: Polygon
F4: Custom Shape
F5: Geometry Data Parser
F6: Dice (3D)
F7: Sphere (3D)

Current View: {_view}";
            font.DrawText(_spriteBatch, text, new Vector2(20f, 20f), Color.White);
            _spriteBatch.End();

            // Prepare effect
            _effect.Color = Color.Green;
            _effect.VertexColorEnabled = true;
            _effect.TextureEnabled = false;
            _effect.ApplyFrom(_camera);

            bool is3D = _view == PrimitiveView.Dice || _view == PrimitiveView.Sphere;
            float scale = is3D ? 200.0f : 200.0f;
            Matrix transform = is3D
                ? Matrix.CreateRotationY(MathHelper.ToRadians((float)(context.GameTime.TotalGameTime.TotalSeconds * 30)))
                : Matrix.CreateScale(scale, scale, 1f);

            // Draw selected primitive
            switch (_view)
            {
                case PrimitiveView.Rectangle:
                    _rectanglePrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Ellipse:
                    _ellipsePrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Polygon:
                    _polygonPrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Custom:
                    _shapePrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Parser:
                    _parsedShapePrimitive.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Dice:
                    _dicePrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
                case PrimitiveView.Sphere:
                    _spherePrimitive?.DrawScoped(context, transform, _camera, _effect);
                    break;
            }
        }
    }
}
