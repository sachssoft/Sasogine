using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Input;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public sealed class FrameContext
    {
        private readonly Workspace _workspace;
        private readonly InputManager _input;
        private readonly IRenderContext _render;

        private GameTime _gameTime;
        private LayoutBounds _bounds;

        public FrameContext(Workspace workspace, InputManager Input, IRenderContext render)
        {
            _workspace = workspace;
            _input = Input;
            _render = render;

            _gameTime = null!;
            _bounds = default;
        }

        public Workspace Workspace => _workspace;

        public InputManager Input => _input;

        public IRenderContext Render => _render;

        public GameTime GameTime => _gameTime;

        public LayoutBounds Bounds => _bounds; // Absolute

        public void Update(GameTime gameTime, LayoutBounds bounds)
        {
            _gameTime = gameTime;
            _bounds = bounds;
        }
    }
}
