using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Example.Primitives
{
    public class PrimitiveExampleApp : MyGameApp<PrimitiveExampleAssets>
    {
        protected override PrimitiveExampleAssets CreateAssetManager()
        {
            return new PrimitiveExampleAssets(this);
        }

        protected override void OnDraw(GameFrameContext context)
        {
            base.OnDraw(context);

            context.GraphicsDevice.Clear(Color.DarkBlue);
        }
    }
}
