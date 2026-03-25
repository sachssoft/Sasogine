using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine.Scenes
{
    public class RuntimeViewportContext : RuntimeContext
    {
        private ICamera _camera;
        //private CameraBase _camera;
        private IEffectAdapter _effectAdapter;

        public RuntimeViewportContext(GameApplication application, RuntimeBase runtime, int viewportIndex)
            : base(application, runtime)
        {
            ViewportIndex = viewportIndex;
        }

        public int ViewportIndex { get; }

        public ICamera Camera
        //public CameraBase Camera
        {
            get => _camera ?? throw new InvalidOperationException("Camera is not set.");
            set => _camera = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IEffectAdapter EffectAdapter
        {
            get => _effectAdapter ?? throw new InvalidOperationException("Effect Adapter is not set.");
            set => _effectAdapter = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Rectangle Viewport => Runtime.GetSplitViewport(ViewportIndex);
    }
}
