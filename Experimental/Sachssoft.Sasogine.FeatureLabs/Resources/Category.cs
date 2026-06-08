using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.FeatureLabs.Resources
{
    public class Category : IResourceTreeEntry
    {

        // Parameterless constructor (für z.B. Deserialisierung)
        public Category()
        {
            Id = Guid.NewGuid().ToString();
            Children = ImmutableHashSet<IResourceTreeEntry>.Empty;
        }

        [SetsRequiredMembers]
        public Category(string id, IEnumerable<IResourceTreeEntry>? children = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Children = (children ?? Enumerable.Empty<IResourceTreeEntry>())
                .Cast<IResourceTreeEntry>()
                .ToImmutableHashSet();
        }

        public required string Id { get; init; }

        public string? Class { get; init; }

        public required IReadOnlySet<IResourceTreeEntry> Children { get; init; }

        public string? Path { get; init; }

        public IResourceTreeEntry? GetChild(string id) =>
            Children.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.Ordinal));

        public IEnumerable<IResourceTreeEntry> GetChildrenByClass(string className) =>
            Children.Where(c => string.Equals(c.Class, className, StringComparison.Ordinal));

        public IEnumerable<IResourceTreeEntry> GetDescendants()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var descendant in child.GetDescendants())
                    yield return descendant;
            }
        }
    }
}
