/* 
 * © 2024 Tobias Sachs
 * GameAssetManager (erweitert)
 * Unterstützt: XNB, EmbeddedResource, Datei (inkl. TTF für FontStashSharp)
*/

using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;

namespace Sachssoft.Sasogine;

public enum AssetSourceType
{
    ExternalFile,
    EmbeddedResource,
    Content
}
public class GameAssetManager
{
    private IMyGameApp _app;
    private Dictionary<string, AssetEntry<Texture2D>> _textures = new();
    private Dictionary<string, AssetEntry<Model>> _models = new();
    private Dictionary<string, AssetEntry<Effect>> _shaders = new();
    private Dictionary<string, AssetEntry<FontSystem>> _fontSystems = new();
    private Dictionary<string, AssetEntry<Stream>> _data = new();

    public GameAssetManager(IMyGameApp app) => _app = app;

    public IMyGameApp App => _app;

    public string? ExternalFileDirectory { get; set; }

    public ExternalFileLocation ExternalFileLocation { get; set; }

    private string GetExternalPath(string entryPath)
    {
        if (string.IsNullOrWhiteSpace(entryPath))
            throw new ArgumentException("Entry path must not be null or empty.", nameof(entryPath));

        return ExternalFileLocation switch
        {
            ExternalFileLocation.Application =>
                Path.Combine(Environment.CurrentDirectory, entryPath),

            ExternalFileLocation.ContentManager =>
                Path.Combine(_app.Content.RootDirectory, entryPath),

            ExternalFileLocation.Custom =>
                !string.IsNullOrWhiteSpace(ExternalFileDirectory) && Path.IsPathRooted(ExternalFileDirectory)
                    ? Path.Combine(ExternalFileDirectory, entryPath)
                    : throw new InvalidOperationException("ExternalFileDirectory is not set or invalid."),

            _ => throw new ArgumentOutOfRangeException(nameof(ExternalFileLocation),
                $"Unsupported file location: {ExternalFileLocation}")
        };
    }

    internal protected virtual void OnLoad() { }

    internal protected virtual void OnUnload() => UnloadAll();

    public void AddTexture(string key, string path, AssetSourceType type = AssetSourceType.Content) =>
        _textures.Add(key, new(path, type));

    public Texture2D AddAndLoadTexture(string key, string path, AssetSourceType type = AssetSourceType.Content)
    {
        AddTexture(key, path, type);
        return LoadTexture(key);
    }

    public void AddModel(string key, string path, AssetSourceType type = AssetSourceType.Content) =>
        _models.Add(key, new(path, type));

    public void AddShader(string key, string path, AssetSourceType type = AssetSourceType.Content) =>
        _shaders.Add(key, new(path, type));

    public void AddFontSystem(string key, string path, AssetSourceType type = AssetSourceType.Content) =>
        _fontSystems.Add(key, new(path, type));

    public void AddData(string key, string path, AssetSourceType type = AssetSourceType.Content) =>
        _data.Add(key, new(path, type));

    public Texture2D LoadTexture(string key)
    {
        if (!_textures.TryGetValue(key, out var entry))
            throw new Exception($"Texture '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        if (entry.SourceType == AssetSourceType.Content)
        {
            entry.Data = _app.Content.Load<Texture2D>(entry.Path);
        }
        else if (entry.SourceType == AssetSourceType.ExternalFile)
        {            
            using var stream = File.OpenRead(GetExternalPath(entry.Path));
            entry.Data = Texture2D.FromStream(_app.GraphicsDevice, stream);
        }
        else
        {
            // EmbeddedResource, etc.
            using var s = GetResourceStream(entry.Path);
            entry.Data = Texture2D.FromStream(_app.GraphicsDevice, s);
        }

        return entry.Data!;
    }

    public Model LoadModel(string key)
    {
        if (!_models.TryGetValue(key, out var entry))
            throw new Exception($"Model '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        if (entry.SourceType == AssetSourceType.Content)
        {
            entry.Data = _app.Content.Load<Model>(entry.Path);
        }
        else if (entry.SourceType == AssetSourceType.ExternalFile)
        {
            throw new NotSupportedException($"Loading models from external files is currently not supported: '{entry.Path}'.");
        }
        else
        {
            throw new NotSupportedException($"Loading models from source type '{entry.SourceType}' is not supported: '{entry.Path}'.");
        }

        return entry.Data!;
    }

    public Effect LoadShader(string key)
    {
        if (!_shaders.TryGetValue(key, out var entry))
            throw new Exception($"Shader '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        switch (entry.SourceType)
        {
            case AssetSourceType.Content:
                entry.Data = _app.Content.Load<Effect>(entry.Path);
                break;

            case AssetSourceType.ExternalFile:
                // Shader von externer Datei (.mgfxo)
                using (var fs = File.OpenRead(GetExternalPath(entry.Path)))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    entry.Data = new Effect(_app.GraphicsDevice, ms.ToArray());
                }
                break;

            case AssetSourceType.EmbeddedResource:
                // Shader als Embedded Resource (z. B. .mgfxo eingebettet)
                using (var stream = GetResourceStream(entry.Path))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    entry.Data = new Effect(_app.GraphicsDevice, ms.ToArray());
                }
                break;

            default:
                throw new NotSupportedException($"Unsupported shader source type: {entry.SourceType}");
        }

        return entry.Data!;
    }


    public Stream LoadData(string key)
    {
        if (!_data.TryGetValue(key, out var entry))
            throw new Exception($"Data asset '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        switch (entry.SourceType)
        {
            case AssetSourceType.Content:
                {
                    // Content pipeline muss Byte[] als rohes Asset ausliefern
                    var bytes = _app.Content.Load<byte[]>(entry.Path);
                    var ms = new MemoryStream(bytes);
                    entry.Data = ms;
                    break;
                }
            case AssetSourceType.ExternalFile:
                {
                    var fs = File.OpenRead(GetExternalPath(entry.Path));
                    entry.Data = fs;
                    break;
                }
            case AssetSourceType.EmbeddedResource:
                {
                    using var s = GetResourceStream(entry.Path);
                    var ms = new MemoryStream();
                    s.CopyTo(ms);
                    ms.Position = 0;
                    entry.Data = ms;
                    break;
                }
            default:
                throw new NotSupportedException($"Unsupported asset source type: {entry.SourceType}");
        }

        return entry.Data;
    }

    public FontSystem LoadFontSystem(string key)
    {
        if (!_fontSystems.TryGetValue(key, out var entry))
            throw new Exception($"FontSystem asset '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        switch (entry.SourceType)
        {
            case AssetSourceType.Content:
                {
                    // Content-Pipeline muss das FontSystem als fertiges Objekt bereitstellen
                    entry.Data = _app.Content.Load<FontSystem>(entry.Path);
                    break;
                }
            case AssetSourceType.ExternalFile:
                {
                    var bytes = File.ReadAllBytes(GetExternalPath(entry.Path));
                    var fs = new FontSystem();
                    fs.AddFont(bytes);
                    entry.Data = fs;
                    break;
                }
            case AssetSourceType.EmbeddedResource:
                {
                    using var stream = GetResourceStream(entry.Path);
                    var bytes = ReadStreamFully(stream);
                    var fs = new FontSystem();
                    fs.AddFont(bytes);
                    entry.Data = fs;
                    break;
                }
            default:
                throw new NotSupportedException($"Unsupported asset source type: {entry.SourceType}");
        }

        return entry.Data!;
    }

    public void UnloadAll()
    {
        foreach (var key in _textures.Keys.ToList())
            UnloadAsset(_textures, key);
        foreach (var key in _models.Keys.ToList())
            UnloadAsset(_models, key);
        foreach (var key in _shaders.Keys.ToList())
            UnloadAsset(_shaders, key);
        foreach (var key in _fontSystems.Keys.ToList())
            UnloadAsset(_fontSystems, key);
        foreach (var key in _data.Keys.ToList())
            UnloadAsset(_data, key);
    }

    public void UnloadTexture(string key) => UnloadAsset(_textures, key);
    public void UnloadModel(string key) => UnloadAsset(_models, key);
    public void UnloadShader(string key) => UnloadAsset(_shaders, key);
    public void UnloadFontSystem(string key) => UnloadAsset(_fontSystems, key);
    public void UnloadData(string key) => UnloadAsset(_data, key);

    private void UnloadAsset<T>(Dictionary<string, AssetEntry<T>> dict, string key)
    {
        if (!dict.TryGetValue(key, out var entry)) return;

        if (entry.Data is IDisposable disposable)
        {
            try { disposable.Dispose(); } catch { /* ignore */ }
        }

        if (entry.SourceType == AssetSourceType.Content)
        {
            try { _app.Content.UnloadAsset(entry.Path); } catch { /* optional */ }
        }

        entry.Data = default!;
    }

    private T LoadAsset<T>(Dictionary<string, AssetEntry<T>> dict, string key, Func<string, T> loader)
    {
        if (!dict.TryGetValue(key, out var entry))
            throw new Exception($"AssetBase '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        switch (entry.SourceType)
        {
            case AssetSourceType.Content:
            case AssetSourceType.ExternalFile:
                entry.Data = loader(entry.Path);
                break;

            case AssetSourceType.EmbeddedResource:
                {
                    using var s = GetResourceStream(entry.Path);
                    if (typeof(T) == typeof(Stream))
                        entry.Data = (T)(object)new MemoryStream(ReadStreamFully(s));
                    else if (typeof(T) == typeof(Texture2D))
                        entry.Data = (T)(object)Texture2D.FromStream(_app.GraphicsDevice, s);
                    else if (typeof(T) == typeof(FontSystem))
                    {
                        var fs = new FontSystem();
                        fs.AddFont(ReadStreamFully(s));
                        entry.Data = (T)(object)fs;
                    }
                    else
                        throw new NotSupportedException("Resource type not supported");
                    break;
                }
        }

        return entry.Data!;
    }

    private Texture2D LoadTextureFunc(string path)
    {
        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(_app.GraphicsDevice, stream);
    }

    private Stream LoadStreamFunc(string path) => File.OpenRead(path);

    private FontSystem LoadFontSystemFunc(string path)
    {
        var fs = new FontSystem();
        fs.AddFont(File.ReadAllBytes(path));
        return fs;
    }

    private Stream GetResourceStream(string filePath)
    {
        var name = (_app.Content.RootDirectory + "/" + filePath).Replace("/", ".").ToLower();
        var assembly = _app.GetType().Assembly;


        foreach (var resource in assembly.GetManifestResourceNames())
        {
#if DEBUG
            Debug.WriteLine(resource);
#endif
            if (resource.ToLower().EndsWith(name, StringComparison.InvariantCultureIgnoreCase))
                return assembly.GetManifestResourceStream(resource) ?? throw new Exception("Stream null");
        }

        throw new Exception("Embedded resource not found: " + filePath);
    }

    public IEnumerable<string> GetResourceEntries()
    {
        var assembly = _app.GetType().Assembly;

        foreach (var resource in assembly.GetManifestResourceNames())
        {
            // RootNamespace entfernen
            string name = resource;
            string root = _app.GetType().Namespace ?? "";
            if (name.StartsWith(root + "."))
                name = name.Substring(root.Length + 1);

            // Punkte in Pfad-Slashes umwandeln, **letzten Punkt vor Extension ignorieren**
            int lastDot = name.LastIndexOf('.');
            if (lastDot > 0)
            {
                string path = name.Substring(0, lastDot).Replace('.', '/');
                string fileName = name.Substring(lastDot + 1);
                yield return path + "." + fileName;
            }
            else
            {
                yield return name.Replace('.', '/');
            }
        }
    }


    private byte[] ReadStreamFully(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private class AssetEntry<T>
    {
        public string Path { get; }
        public AssetSourceType SourceType { get; }
        public T? Data { get; set; }

        public AssetEntry(string path, AssetSourceType sourceType)
        {
            Path = path;
            SourceType = sourceType;
        }
    }
}