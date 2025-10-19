using FontStashSharp;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleAssets : GameAssetManager
    {
        private const string FONT_TEXT = "font_text";

        private FontSystem? _textFont;

        public SurfaceExampleAssets(IMyGameApp app) : base(app)
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
