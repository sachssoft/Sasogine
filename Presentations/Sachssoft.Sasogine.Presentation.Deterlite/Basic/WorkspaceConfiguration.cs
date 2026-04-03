using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using Sachssoft.Sasogine.Presentation.Deterlite.Styling;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public class WorkspaceConfiguration
    {

        public WorkspaceConfiguration() { }

        public IRenderContext? RenderContext { get; init; }

        public Skin? Skin { get; init; }

        public StyleRegistry? StyleRegistry { get; init; }

        public string? EmbeddedRootPath { get; init; }

        public string? LocalRootPath { get; init; }

        //public ValueRegistry? ValueRegistry { get; init; }

        //public float DesignWidth { get; set; } = 1920f;

        //public float DesignHeight { get; set; } = 1080f;

    }
}
