namespace Sachssoft.Sasogine.Presentation.Styling
{
    public interface ITypeFactory<T, TEntry> 
        where T : class
        where TEntry : class, ISkinEntry
    {
        T Create(Skin skin, TEntry entry);
    }
}
