
namespace Sachssoft.Sasogine.Resources;

public interface ITypeFactory<T, TEntry> 
    where TEntry : class, IResourceEntry
{
    T Create(ResourceStore store, TEntry entry);
}
