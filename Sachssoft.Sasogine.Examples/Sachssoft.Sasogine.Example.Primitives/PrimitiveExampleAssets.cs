using FontStashSharp;

namespace Sachssoft.Sasogine.Example.Primitives
{
    public class PrimitiveExampleAssets : GameAssetManager
    {
        private const string FONT_TEXT = "font_text";

        private FontSystem? _textFont;

        public PrimitiveExampleAssets(IMyGameApp app) : base(app)
        {
            AddFontSystem(FONT_TEXT, "fonts/RobotoCondensed-Regular.ttf", AssetSourceType.EmbeddedResource);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _textFont = LoadFontSystem(FONT_TEXT);
        }

        public DynamicSpriteFont GetFont(float fontSize) => _textFont!.GetFont(fontSize);
    }
}
