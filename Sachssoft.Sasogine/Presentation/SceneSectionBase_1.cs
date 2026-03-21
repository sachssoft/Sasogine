namespace Sachssoft.Sasogine.Presentation
{
    public abstract class SceneSectionBase<TScene> : SceneSectionBase
        where TScene : SceneBase
    {
        private readonly TScene _owner;

        public SceneSectionBase(TScene owner)
            : base(owner)
        {
            _owner = owner;
        }

        protected new TScene Owner => _owner;
    }
}
