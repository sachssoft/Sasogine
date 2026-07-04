using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public sealed class Skin : ResourceStore, IStyleContainer
    {
        private readonly Workspace _workspace;

        private readonly List<TextureAtlasSet> _textureAtlasSets = new();
        private readonly List<FontFaceSet> _fontFaceSets = new();

        public Skin(Workspace workspace, IEnumerable<IResourceEntry> entries, ResourceFileSource source, string? rootPath = null)
            : base(workspace.Application, entries, source, rootPath)
        {
            _workspace = workspace;
        }

        public Workspace Workspace => _workspace;

        public List<StylesheetNamespace> Namespaces { get; } = new();

        public IReadOnlyCollection<TextureAtlasSet> TextureAtlasSets => _textureAtlasSets.AsReadOnly();

        public IReadOnlyCollection<FontFaceSet> FontFaceSets => _fontFaceSets.AsReadOnly();

        public void Load()
        {
            foreach (var entry in Entries)
            {
                if (entry is Resource resource)
                {
                    // Erst die Resource laden
                    resource.Load(this);

                    // Danach die Instanz holen
                    switch (resource.TargetType)
                    {
                        case Type t when t == typeof(TextureAtlasSet):
                            _textureAtlasSets.Add(resource.GetInstance<TextureAtlasSet>());
                            break;

                        case Type t when t == typeof(FontFaceSet):
                            _fontFaceSets.Add(resource.GetInstance<FontFaceSet>());
                            break;
                    }
                }
            }
        }

        public void Unload()
        {
        }

        public bool TryGetStyle<T>(string? id, [MaybeNullWhen(false)] out Style style)
            where T : class, IStyleable
        {
            style = null;

            if (string.IsNullOrEmpty(id))
                return false;

            // Suche Style nach TargetType und Id
            style = Entries.OfType<Style>()
                           .FirstOrDefault(s =>
                               s.TargetType == typeof(T) &&
                               string.Equals(s.Id, id, StringComparison.Ordinal)
                           );

            return style != null;
        }
    }
}
