namespace Sachssoft.Sasogine.Surface.Styles
{
    public interface IStyleFactory<T> where T : class
    {
        static abstract string PrefixName { get; }

        static abstract T Create(StyleFactoryContext<T> context) ;

    }
}
