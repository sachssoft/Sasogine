using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace sachssoft.Sasogine.Elements;

[DataContract]
public abstract class GameObject : IGameObject, INotifyPropertyChanged, INotifyPropertyChanging, ICloneable
{
    private bool _locked;
    private string? _id;
    private string? _name;
    private string? _class;
    private object? _data_context;
    private GameObject? _parent;

    private object? _last_new_value;
    private object? _last_old_value;
    private string? _lastPropertyName;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;

    protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;

        _last_old_value = field;
        _last_new_value = value;
        _lastPropertyName = propertyName;

        OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
        field = value;
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        return true;
    }

    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        => PropertyChanging?.Invoke(this, e);

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        => PropertyChanged?.Invoke(this, e);

    // Letzte Änderung
    internal string? LastPropertyName => _lastPropertyName;
    internal object? LastOldValue => _last_old_value;
    internal object? LastNewValue => _last_new_value;

    public GameObject GetRoot()
    {
        GameObject current = this;
        while (current._parent != null)
            current = current._parent;
        return current;
    }

    public T? FindParent<T>(int index = 0) where T : GameObject
    {
        GameObject? current = _parent;
        int count = 0;
        while (current != null)
        {
            if (current is T match)
            {
                if (count == index)
                    return match;
                count++;
            }
            current = current._parent;
        }
        return null;
    }

    public T? FindParent<T>(Func<T, bool>? predicate) where T : GameObject
    {
        GameObject? current = _parent;
        while (current != null)
        {
            if (current is T match && (predicate?.Invoke(match) ?? true))
                return match;

            current = current._parent;
        }
        return null;
    }

    object ICloneable.Clone() => Clone();

    public virtual GameObject Clone()
    {
        var copy = (GameObject)MemberwiseClone();
        copy._locked = false;
        copy._parent = null;
        return copy;
    }

    [IgnoreDataMember]
    [Browsable(false)]
    public GameObject? Parent
    {
        get => _parent;
        internal set => RaiseAndSetIfChanged(ref _parent, value);
    }

    [DataMember]
    public virtual string? ID
    {
        get => _id;
        set => RaiseAndSetIfChanged(ref _id, value);
    }

    [DataMember]
    public virtual string? Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    [DataMember]
    public virtual string? Class
    {
        get => _class;
        set => RaiseAndSetIfChanged(ref _class, value);
    }

    [IgnoreDataMember]
    [Browsable(false)]
    public virtual object? DataContext
    {
        get => _data_context;
        set => RaiseAndSetIfChanged(ref _data_context, value);
    }
}
