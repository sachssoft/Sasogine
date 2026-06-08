using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.FeatureLabs.Resources;

public interface IResourceContainer
{
    bool TryGetResource<T>(string id, [MaybeNullWhen(false)] out T value, ResourceLookupOptions options = ResourceLookupOptions.None);
}
