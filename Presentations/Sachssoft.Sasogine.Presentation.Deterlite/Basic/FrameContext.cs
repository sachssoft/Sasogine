using Sachssoft.Sasogine.Presentation.Deterlite.Input;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public sealed class FrameContext
    {
        private readonly Workspace _workspace;
        private readonly InputManager _input;
        private readonly IRenderContext _render;

        public FrameContext(Workspace workspace, InputManager Input, IRenderContext render)
        {
            _workspace = workspace;
            _input = Input;
            _render = render;
        }

        public Workspace Workspace => _workspace;

        public InputManager Input => _input;

        public IRenderContext Render => _render;
    }
}
