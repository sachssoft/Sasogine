using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Sachssoft.Sasogine.Elements;

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

    public GameObject? Owner => _owner;

    public bool IDGenerationAllowed => _id_generator_prefix != null;

    public string? IDGeneratorPrefix => _id_generator_prefix;

    public void GenerateNewID(T obj, string prefix)
    {
        if (string.IsNullOrWhiteSpace(obj.ID))
        {
            // Alle existierenden IDs mit dem Präfix sammeln
            var existingNumbers = this
                .Where(x => !string.IsNullOrWhiteSpace(x.ID) && x.ID.StartsWith(prefix))
                .Select(x =>
                {
                    var suffix = x.ID?.Substring(prefix.Length);
                    return int.TryParse(suffix, out var n) ? n : 0;
                })
                .ToHashSet();

            // Startnummer
            int number = 1;

            // Solange hochzählen, bis freie Nummer gefunden ist
            while (existingNumbers.Contains(number))
            {
                number++;
            }

            obj.ID = $"{prefix}{number}";
        }
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
            ResetItem(item);
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
            throw new InvalidOperationException("Item is already assigned to a different collection.");
    }

    public void ForEach(Action<T> a)
    {
        foreach (var item in this)
            a.Invoke(item);
    }

    public IEnumerable<T> GetByID(string? id) =>
        this.Where(item => item.ID == id);

    public IEnumerable<T> GetByClass(string? name) =>
        this.Where(item => (item is IGameObject obj) && obj.Class == name);

    public bool ContainsAmbiguousID(out string[] ids)
    {
        // Bei Weglassen der Eindeutigkeit kann man alle IDs zurückgeben, die mehrfach vorkommen
        var duplicates = this.GroupBy(i => i.ID)
                             .Where(g => g.Count() > 1 && g.Key != null)
                             .Select(g => g.Key!)
                             .ToArray();
        ids = duplicates;
        return ids.Length > 0;
    }

    public T[] FindByID(string? id) =>
        GetByID(id).ToArray();

    public T[] FindByClass(string? name) =>
        GetByClass(name).ToArray();
}
