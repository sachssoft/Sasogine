using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Verwaltet eine Sammlung von Entities.
    /// </summary>
    public class EntityCollection : IList<IEntity>
    {
        private readonly List<IEntity> _entities = new();
        private readonly List<IEntity> _sortedCache = new();

        private bool _cacheDirty = true;

        /// <summary>
        /// Liefert alle Entities in Render-/Update-Reihenfolge.
        /// Nicht geordnete Entities werden am Ende einsortiert.
        /// </summary>
        public IReadOnlyList<IEntity> OrderedEntities
        {
            get
            {
                UpdateCache();
                return _sortedCache;
            }
        }

        public IEntity this[int index]
        {
            get => _entities[index];
            set
            {
                _entities[index] = value;
                _cacheDirty = true;
            }
        }

        public int Count => _entities.Count;

        public bool IsReadOnly => false;

        public void Add(IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _entities.Add(item);
            _cacheDirty = true;
        }

        public void Clear()
        {
            _entities.Clear();
            _sortedCache.Clear();
            _cacheDirty = true;
        }

        public bool Contains(IEntity item)
            => _entities.Contains(item);

        public void CopyTo(IEntity[] array, int arrayIndex)
            => _entities.CopyTo(array, arrayIndex);

        public IEnumerator<IEntity> GetEnumerator()
            => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public int IndexOf(IEntity item)
            => _entities.IndexOf(item);

        public void Insert(int index, IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _entities.Insert(index, item);
            _cacheDirty = true;
        }

        public bool Remove(IEntity item)
        {
            bool removed = _entities.Remove(item);

            if (removed)
                _cacheDirty = true;

            return removed;
        }

        public void RemoveAt(int index)
        {
            _entities.RemoveAt(index);
            _cacheDirty = true;
        }

        public void Load()
        {
            ForEachOrdered(e => e.Load());
        }

        public async Task LoadAsync()
        {
            UpdateCache();

            foreach (var entity in _sortedCache)
            {
                await entity.LoadAsync().ConfigureAwait(false);
            }
        }

        public void Unload()
        {
            UpdateCache();

            for (int i = _sortedCache.Count - 1; i >= 0; i--)
            {
                _sortedCache[i].Unload();
            }
        }

        public void Update(SceneUpdateContext context)
        {
            ForEachOrdered(e =>
            {
                if (e is IUpdatableEntity updatable)
                    updatable.Update(context);
            });
        }

        public void Draw(SceneDrawContext context)
        {
            ForEachOrdered(e =>
            {
                if (e is IDrawableEntity drawable)
                    drawable.Draw(context);
            });
        }

        private void ForEachOrdered(Action<IEntity> action)
        {
            UpdateCache();

            foreach (var entity in _sortedCache)
                action(entity);
        }

        private void UpdateCache()
        {
            if (!_cacheDirty)
                return;

            _sortedCache.Clear();
            _sortedCache.AddRange(_entities);

            _sortedCache.Sort((a, b) =>
            {
                int oa = a is IOrderedEntity ao ? ao.Order : int.MaxValue;
                int ob = b is IOrderedEntity bo ? bo.Order : int.MaxValue;

                int result = oa.CompareTo(ob);
                if (result != 0)
                    return result;

                // stabile Reihenfolge
                return _entities.IndexOf(a).CompareTo(_entities.IndexOf(b));
            });

            _cacheDirty = false;
        }
    }
}