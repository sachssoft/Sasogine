using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Text.Json;
using sachssoft.Sasogine.Core;
using sachssoft.Sasogine.Elements;
using sachssoft.Sasogine.Stream.Json.Converters;

namespace sachssoft.Sasogine;

public class GameSettings : GameObject 
{
    private Dictionary<string, object?> _settings;

    public GameSettings()
    {
    }

    public GameSettings(string filename)
    {
        _settings = new();
        Filename = filename;
    }

    [AllowNull]
    public string Filename
    {
        get;
    }

    protected void LoadFromStorage()
    {
        var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        var stream = new IsolatedStorageFileStream(Filename, System.IO.FileMode.OpenOrCreate, store);

        LoadFromStream(stream);
        stream.Close();
    }

    protected void LoadFromPath(string path)
    {
        var stream = new FileStream($"{path}{Path.DirectorySeparatorChar}{Filename}", FileMode.Open);

        LoadFromStream(stream);
        stream.Close();
    }

    protected void LoadFromStream(System.IO.Stream stream)
    {
        try
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ObjectJsonConverter());
            _settings = JsonSerializer.Deserialize<Dictionary<string, object?>>(stream, options) ?? new();
        }
        catch (JsonException)
        {
            _settings = new();
        }

    }

    protected void SaveToStorage()
    {
        var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        var stream = new IsolatedStorageFileStream(Filename, System.IO.FileMode.Create, store);

        SaveToStream(stream);
        stream.Close();
    }

    protected void SaveToPath(string path)
    {
        var stream = new FileStream($"{path}{Path.DirectorySeparatorChar}{Filename}", FileMode.Create);

        SaveToStream(stream);
        stream.Close();
    }

    protected void SaveToStream(System.IO.Stream stream)
    {
        JsonSerializer.Serialize(stream, _settings);
    }

    internal protected virtual void OnLoad()
    {
    }

    internal protected virtual void OnSave()
    {
    }

    public T? GetValue<T>([CallerMemberName] string property_name = "", T? default_value = default, Func<object?, T>? converter = null)
    {
        if (_settings.TryGetValue(property_name, out var v))
        {
            try
            {
                if (converter != null)
                {
                    return converter.Invoke(v);
                }
                else
                {
                    return (T?)v;
                }
            }
            catch
            {
                Debug.WriteLine("GameSettings Invalid Type Warning: " + property_name);
                _settings.Remove(property_name);
            }
        }

        return default_value;
    }

    public void SetValue<T>([CallerMemberName] string property_name = "", T? value = default)
    {
        _settings[property_name] = value;
    }

    public void Clear()
    {
        _settings.Clear();
    }

    public void Reset(string key)
    {
        _settings.Remove(key);
    }

    public virtual void Apply(IMyGameApp app)
    {
    }
}
