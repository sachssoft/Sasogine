namespace Sachssoft.Sasogine.Resources;

public interface IResourceContainer
{
    bool TryGetResource<T>(string id, out T value);
}
