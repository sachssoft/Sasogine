/* 
 * © 2024 Tobias Sachs
 * GameObjectCollection
 * 11.07.2024 
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace sachssoft.Sasogine.Elements;

public class GameObjectCollection<T> : ObservableCollection<T> where T : IIdentifiable
{
    private GameObject? _owner;
    private string? _id_generator_prefix;

    public event EventHandler<ItemChangedEventArgs>? ItemPropertyChanged;

    public GameObjectCollection()
        : this(null, null) { }

    public GameObjectCollection(GameObject? owner)
        : this(owner, null) { }

    public GameObjectCollection(GameObject? owner, string? id_generator_prefix)
    {
        _owner = owner;
        _id_generator_prefix = id_generator_prefix;
    }

    public GameObject? Owner
    {
        get => _owner;
        set => _owner = value;
    }

    public bool IDGenerationAllowed => _id_generator_prefix != null;

    public string? IDGeneratorPrefix => _id_generator_prefix;

    public void GenerateNewID(T obj, string prefix)
    {
        if (string.IsNullOrWhiteSpace(obj.ID))
        {
            obj.ID = GetNewID(prefix);
        }
    }

    public string GetNewID(string prefix)
    {
        var existingIDs = new HashSet<string>(this.Select(x => x.ID).Where(id => id != null)!);
        int n = 1;
        string newId;
        do
        {
            newId = $"{prefix}{n++}";
        } while (existingIDs.Contains(newId));
        return newId;
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var item = (T)sender!;
        object? old_value = null;
        object? new_value = null;

        if (item is GameObject obj)
        {
            old_value = obj.LastOldValue;
            new_value = obj.LastNewValue;
        }

        ItemPropertyChanged?.Invoke(this, new ItemChangedEventArgs(item, old_value, new_value));
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        var n = index;
        foreach (var item in items)
        {
            Insert(n++, item);
        }
    }

    protected override sealed void InsertItem(int index, T item)
    {
        CheckHasParent(item);
        SetupItem(item);
        base.InsertItem(index, item);
    }

    protected override sealed void ClearItems()
    {
        foreach (var item in this)
        {
            ResetItem(item);
        }
        base.ClearItems();
    }

    protected override sealed void SetItem(int index, T item)
    {
        CheckHasParent(item);
        ResetItem(this[index]);
        SetupItem(item);
        base.SetItem(index, item);
    }

    protected override sealed void RemoveItem(int index)
    {
        ResetItem(this[index]);
        base.RemoveItem(index);
    }

    private void SetupItem(T item)
    {
        if (item is GameObject obj)
        {
            obj.Parent = _owner;
            obj.PropertyChanged += Item_PropertyChanged;
        }
        ItemOnSetup(item);
        AllowsToGenerateID(item);
    }

    protected virtual void ItemOnSetup(T item) { }

    private void ResetItem(T item)
    {
        ItemOnReset(item);
        if (item is GameObject obj)
        {
            obj.Parent = null;
            obj.PropertyChanged -= Item_PropertyChanged;
        }
    }

    protected virtual void ItemOnReset(T item) { }

    private void AllowsToGenerateID(T item)
    {
        if (_id_generator_prefix != null)
        {
            GenerateNewID(item, _id_generator_prefix);
        }
    }

    private void CheckHasParent(IIdentifiable item)
    {
        if (item is GameObject obj && obj.Parent != null && obj.Parent != _owner)
            throw new InvalidOperationException("Item already contains in collection");
    }

    public void ForEach(Action<T> a)
    {
        foreach (var item in this)
        {
            a.Invoke(item);
        }
    }

    public IEnumerable<T> GetByID(string? id)
    {
        return this.Where(item => item.ID == id);
    }

    public IEnumerable<T> GetByClass(string? name)
    {
        return this.Where(item => (item is IGameObject obj) && obj.Class == name);
    }

    public bool ContainsAmbiguousID(out string[] ids)
    {
        var seen = new HashSet<string?>();
        var duplicates = new HashSet<string>();
        foreach (var item in this)
        {
            if (item.ID != null && !seen.Add(item.ID))
                duplicates.Add(item.ID);
        }
        ids = duplicates.ToArray();
        return duplicates.Count > 0;
    }

    public T[] FindByID(string? id) => this.Where(item => item.ID == id).ToArray();

    public T[] FindByClass(string? name) => this.Where(item => (item is IGameObject obj) && obj.Class == name).ToArray();
}
