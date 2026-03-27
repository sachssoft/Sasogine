using FontStashSharp;
using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleResources : GameResourceManager
    {
        private const string FONT_TEXT = "font_text";

        private FontSystem? _textFont;

        public SurfaceExampleResources(GameApplication app) : base(app)
        {            
        }

        public override void OnInitialize()
        {
            Add<FontSystem>(FONT_TEXT, "fonts/RobotoCondensed-Regular.ttf", ResourceLoaderType.EmbeddedResource);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            _textFont = Load<FontSystem>(FONT_TEXT);
        }

        public DynamicSpriteFont GetFont(float fontSize) => _textFont!.GetFont(fontSize);
    }
}
