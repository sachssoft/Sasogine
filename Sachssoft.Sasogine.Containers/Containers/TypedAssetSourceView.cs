using Sachssoft.Sasogine.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Generische, dynamische Ansicht auf eine IAssetSourceCollection für einen bestimmten Asset-Typ T.
    /// Berücksichtigt Subtypen und reagiert auf AssetChanged.
    /// </summary>
    public sealed class TypedAssetSourceView<T> : IAssetCollection<T> where T : IAsset
    {
        private readonly IAssetSourceCollection _source;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public TypedAssetSourceView(IAssetSourceCollection source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            // Änderungen in der Source weiterleiten
            if (_source is INotifyCollectionChanged ncc)
                ncc.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);

            if (_source is INotifyPropertyChanged npc)
                npc.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);

            // AssetChanged einzelner Assets abonnieren
            foreach (var assetSource in _source)
                assetSource.AssetChanged += (_, _) => OnAssetChanged(assetSource);

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
            if (source.Asset is T asset)
            {
                // CollectionChanged auslösen
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    asset,
                    oldItem: null
                ));

                // PropertyChanged für Bindings
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(source.Asset)));
            }
        }

        /// <summary>
        /// Enumeriert alle Assets aus der Source, die nicht null sind und vom Typ T oder Subtypen.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var assetSource in _source)
            {
                if (assetSource.Asset is T asset)
                    yield return asset;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
