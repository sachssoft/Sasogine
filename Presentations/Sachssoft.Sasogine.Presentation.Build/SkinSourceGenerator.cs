using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Sachssoft.Sasogine.Presentation.Build
{
    [Generator]
    public class SkinSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // optional: Debugger.Launch() für Debugging
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Hardcoded Liste bekannter Typen (kann auch aus Projekt/Attribute generiert werden)
            var sourceCode = @"
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public static partial class SkinRegistryGenerated
    {
        internal static readonly Dictionary<string, Type> s_typeLookup = new()
        {
            { ""Texture2D"", typeof(Texture2D) },
            { ""FontFace"", typeof(FontFace) },
            { ""FontFaceSet"", typeof(FontFaceSet) },
            { ""SolidColorBrush"", typeof(SolidColorBrush) },
            { ""TextureAtlas"", typeof(TextureAtlas) },
            { ""TextureAtlasSet"", typeof(TextureAtlasSet) },
            { ""TextureBrush"", typeof(TextureBrush) }
        };

        internal static readonly string[] s_namespaces = new[]
        {
            ""Microsoft.Xna.Framework.Graphics"",
            ""Sachssoft.Sasogine.Presentation.Rendering"",
            ""Sachssoft.Sasogine.Presentation.Styling""
        };
    }
}";
            context.AddSource("SkinRegistry.Generated.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }
}