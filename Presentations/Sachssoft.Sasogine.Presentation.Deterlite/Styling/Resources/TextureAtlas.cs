#if SASOGINE
global using TextureAtlas = Sachssoft.Sasogine.Resources.TextureAtlas;
#else
global using TextureAtlas = Sachssoft.Sasogine.Presentation.Deterlite.Styling.TextureAtlas;
#endif

#if !SASOGINE
namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public class TextureAtlas
    {
        // Implement your own TextureAtlas class here if not using Sasogine.

        public TextureAtlas()
        {
            // Constructor implementation
        }
        
        public static TextureAtlas FromFile(string filePath)
        {
            throw new NotImplementedException("TextureAtlas loading from file is not implemented.");
        }

        public static TextureAtlas FromResource(string resourceName)
        {
            throw new NotImplementedException("TextureAtlas loading from resource is not implemented.");
        }

    }
}
#endif