using Sachssoft.Sasogine.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Dynamische Ansicht auf eine IAssetSourceCollection mit optionalem Typfilter.
    /// Berücksichtigt Subtypen und reagiert auf AssetChanged.
    /// </summary>
    public sealed class AssetSourceView : IAssetCollection
    {
        private readonly IAssetSourceCollection _source;
        private readonly Type? _filterType;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly AssetSourceView Empty = new AssetSourceView(new EmptyAssetSourceCollection());

        public AssetSourceView(IAssetSourceCollection source, Type? filterType = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _filterType = filterType;

            // Änderungen in der Source weiterleiten
            if (_source is INotifyCollectionChanged ncc)
                ncc.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);

            if (_source is INotifyPropertyChanged npc)
                npc.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);

            // AssetChanged einzelner Assets abonnieren
            foreach (var assetSource in _source)
            {
                assetSource.AssetChanged += (_, _) => OnAssetChanged(assetSource);
            }

            // Neue AssetSources automatisch abonnieren
            if (_source is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += (s, e) =>
                {
                    if (e.NewItems != null)
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            if (newItem is IAssetSource newSource)
                                newSource.AssetChanged += (_, _) => OnAssetChanged(newSource);
                        }
                    }
                };
            }
        }

        private void OnAssetChanged(IAssetSource source)
        {
            // CollectionChanged auslösen
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                source.Asset,
                oldItem: null
            ));

            // PropertyChanged für Bindings
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(source.Asset)));
        }

        /// <summary>
        /// Enumeriert alle Assets aus der Source, die nicht null sind und den Typfilter erfüllen.
        /// Berücksichtigt Subtypen.
        /// </summary>
        public IEnumerator<IAsset> GetEnumerator()
        {
            foreach (var assetSource in _source)
            {
                var asset = assetSource.Asset;
                if (asset != null)
                {
                    // Filter: Subtypen berücksichtigen
                    if (_filterType == null || _filterType.IsInstanceOfType(asset))
                        yield return asset;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class EmptyAssetSourceCollection : IAssetSourceCollection, INotifyCollectionChanged, INotifyPropertyChanged, IEnumerable<IAssetSource>
        {
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
            public event PropertyChangedEventHandler? PropertyChanged;

            public IEnumerator<IAssetSource> GetEnumerator() => Array.Empty<IAssetSource>().AsEnumerable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
