using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public interface IStyleContainer
    {
        bool TryGetStyle<T>(string id, [MaybeNullWhen(false)] out Style style)
            where T : class, IStyleable;
    }
}
