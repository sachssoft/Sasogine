//using Microsoft.Xna.Framework;
//using Sachssoft.Sasogine.Components.Rendering.Camera;
//using Sachssoft.Sasogine.Graphics.Rendering;
//using System;

//namespace Sachssoft.Sasogine.Scenes
//{
//    public class SceneViewportContext : SceneContext
//    {
//        public SceneViewportContext(
//            IGameApplication application, 
//            ViewportSceneBase scene, 
//            int viewportIndex, 
//            ICamera camera,
//            IEffectAdapter effectAdapter,
//            float frameCounterSmoothing = 0.1f, 
//            float frameCounterFastWeight = 0.2f)
//            : base(application, scene, camera, effectAdapter, frameCounterSmoothing, frameCounterFastWeight)
//        {
//            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
//            ViewportIndex = viewportIndex;
//        }

//        public new ViewportSceneBase Scene { get; }

//        public int ViewportIndex { get; }

//        public Rectangle Viewport => Scene.GetSplitViewport(ViewportIndex);
//    }
//}
