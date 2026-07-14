using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation
{
    public class WorkspaceConfiguration
    {

        public WorkspaceConfiguration() { }

        public Skin? Skin { get; init; }

        public IWorkspaceProvider? Provider { get; init; }

        public StyleRegistry? StyleRegistry { get; init; }

        public string? EmbeddedRootPath { get; init; }

        public string? LocalRootPath { get; init; }

        //public ValueRegistry? ValueRegistry { get; init; }

        //public float DesignWidth { get; set; } = 1920f;

        //public float DesignHeight { get; set; } = 1080f;

    }
}
