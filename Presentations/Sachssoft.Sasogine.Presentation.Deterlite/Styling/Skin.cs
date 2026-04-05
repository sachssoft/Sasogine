using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public class Skin
    {
        private readonly Dictionary<string, ISkinEntry> _entries = new();
        private readonly Workspace _workspace;

        private readonly List<TextureAtlasSet> _textureAtlasSets = new();
        private readonly List<FontFaceSet> _fontFaceSets = new();

        public Skin(Workspace workspace)
        {
            _workspace = workspace;
        }

        public Workspace Workspace => _workspace;

        public List<StylesheetNamespace> Namespaces { get; } = new();

        public SkinRegistry Registry { get; } = new();

        public IEnumerable<ISkinEntry> Entries => _entries.Values;

        public IReadOnlyCollection<TextureAtlasSet> TextureAtlasSets => _textureAtlasSets.AsReadOnly();

        public IReadOnlyCollection<FontFaceSet> FontFaceSets => _fontFaceSets.AsReadOnly();

        public virtual void Load()
        {
            // In XML müssen zuerst die FontFace-Resource eingetragen werden, dann FontFamily
            // Umgekehrt würde FontFamily Face nicht finden
            // Es ist Von-Oben-Nach-Unten-Prinzip

            foreach (var entry in _entries.Values)
            {
                if (entry is Resource resource)
                {
                    switch (resource.TargetType)
                    {
                        case Type t when t == typeof(TextureAtlas):
                            {
                                _textureAtlasSets.Add(resource.Create<TextureAtlasSet>(this));
                                break;
                            }
                        case Type t when t == typeof(FontFaceSet):
                            {
                                _fontFaceSets.Add(resource.Create<FontFaceSet>(this));
                                break;
                            }
                    }
                }
            }
        }

        public void Unload()
        {
        }

        public void AddEntry(ISkinEntry entry)
        {
            if (!_entries.TryAdd(entry.Id, entry))
                throw new InvalidOperationException();
        }

        public Resource? FindResource(string id) => 
            Entries.Where(x => x.Id == id && x is Resource)
                   .Cast<Resource>()
                   .FirstOrDefault();

    }
}
