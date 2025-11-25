using System;

namespace Sachssoft.Sasogine.Presentation
{
    public class PresentationContext : GameContext
    {
        private readonly SceneBase _scene;
        private bool _isVisible = true;

        public PresentationContext(GameApplication app, SceneBase scene) : base(app)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        public SceneBase Scene => _scene;

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }
    }
}
