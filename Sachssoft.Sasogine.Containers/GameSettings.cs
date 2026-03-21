using Sachssoft.Sasofly.Inspection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;

namespace Sachssoft.Sasogine;

public class GameSettings : NotifyingObject
{
    public GameSettings()
    {
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
        //try
        //{
        //    var options = new JsonSerializerOptions();
        //    options.Converters.Add(new ObjectJsonConverter());
        //    _settings = JsonSerializer.Deserialize<Dictionary<string, object?>>(stream, options) ?? new();
        //}
        //catch (JsonException)
        //{
        //    _settings = new();
        //}

        throw new NotImplementedException("Implementing later...");
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
        throw new NotImplementedException("Implementing later...");
    }

    internal protected virtual void OnLoad()
    {
    }

    internal protected virtual void OnSave()
    {
    }

    public virtual void Apply(IGameApplication app)
    {
    }
}
