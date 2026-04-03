namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public interface ITypeFactory<T, TEntry> 
        where T : class
        where TEntry : SkinEntry
    {
        T Create(Skin skin, TEntry entry);
    }
}
