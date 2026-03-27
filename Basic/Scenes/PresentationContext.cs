using System;

namespace Sachssoft.Sasogine.Scenes
{
    public class PresentationContext : GameContext
    {
        private readonly IScene _scene;
        private bool _isVisible = true;

        public PresentationContext(IGameApplication app, ISceneWithPresentation scene) : base(app)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        public IScene Scene => _scene;

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }
    }
}
