namespace Sachssoft.Sasogine.Geometry
{
    public abstract class ShapePathBase
    {
        private Path _definedPath;

        protected ShapePathBase()
        {
            _definedPath = BuildDefinedPath();
        }

        public void Rebuild()
        {
            _definedPath = BuildDefinedPath();
        }

        protected abstract Path BuildDefinedPath();

        public Path DefinedPath => _definedPath;
    }
}
