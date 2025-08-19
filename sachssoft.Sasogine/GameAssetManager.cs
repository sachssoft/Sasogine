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
using System.IO;
using System.Linq;

namespace Sachssoft.Sasogine;

public enum AssetSourceType
{
    ExternalFile,
    EmbeddedResource,
    ContentXnb
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

    internal protected virtual void OnLoad() { }
    
    internal protected virtual void OnUnload() => UnloadAll();

    public void AddTexture(string key, string path, AssetSourceType type = AssetSourceType.ContentXnb) =>
        _textures.Add(key, new(path, type));

    public void AddModel(string key, string path, AssetSourceType type = AssetSourceType.ContentXnb) =>
        _models.Add(key, new(path, type));

    public void AddShader(string key, string path, AssetSourceType type = AssetSourceType.ContentXnb) =>
        _shaders.Add(key, new(path, type));

    public void AddFontSystem(string key, string path, AssetSourceType type = AssetSourceType.ContentXnb) =>
        _fontSystems.Add(key, new(path, type));

    public void AddData(string key, string path, AssetSourceType type = AssetSourceType.ContentXnb) =>
        _data.Add(key, new(path, type));

    public Texture2D LoadTexture(string key)
    {
        if (!_textures.TryGetValue(key, out var entry))
            throw new Exception($"Texture '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        if (entry.SourceType == AssetSourceType.ContentXnb)
        {
            entry.Data = _app.Content.Load<Texture2D>(entry.Path);
        }
        else if (entry.SourceType == AssetSourceType.ExternalFile)
        {
            using var stream = File.OpenRead(entry.Path);
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

        if (entry.SourceType == AssetSourceType.ContentXnb)
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

        if (entry.SourceType == AssetSourceType.ContentXnb)
        {
            entry.Data = _app.Content.Load<Effect>(entry.Path);
        }
        else if (entry.SourceType == AssetSourceType.ExternalFile)
        {
            throw new NotSupportedException("Loading shaders from external files is not supported.");
        }
        else // EmbeddedResource etc.
        {
            throw new NotSupportedException("Loading shaders from embedded resources is not supported.");
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
            case AssetSourceType.ContentXnb:
                {
                    // Content pipeline muss Byte[] als rohes Asset ausliefern
                    var bytes = _app.Content.Load<byte[]>(entry.Path);
                    var ms = new MemoryStream(bytes);
                    entry.Data = ms;
                    break;
                }
            case AssetSourceType.ExternalFile:
                {
                    var fs = File.OpenRead(entry.Path);
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
            case AssetSourceType.ContentXnb:
                {
                    // Content-Pipeline muss das FontSystem als fertiges Objekt bereitstellen
                    entry.Data = _app.Content.Load<FontSystem>(entry.Path);
                    break;
                }
            case AssetSourceType.ExternalFile:
                {
                    var bytes = File.ReadAllBytes(entry.Path);
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

        if (entry.SourceType == AssetSourceType.ContentXnb)
        {
            try { _app.Content.UnloadAsset(entry.Path); } catch { /* optional */ }
        }

        entry.Data = default!;
    }

    private T LoadAsset<T>(Dictionary<string, AssetEntry<T>> dict, string key, Func<string, T> loader)
    {
        if (!dict.TryGetValue(key, out var entry))
            throw new Exception($"Asset '{key}' not found.");

        if (entry.Data != null)
            return entry.Data;

        switch (entry.SourceType)
        {
            case AssetSourceType.ContentXnb:
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

    private Stream GetResourceStream(string file_path)
    {
        var name = (_app.Content.RootDirectory + "/" + file_path).Replace("/", ".").ToLower();
        var assembly = _app.GetType().Assembly;

        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (resource.ToLower().EndsWith(name))
                return assembly.GetManifestResourceStream(resource) ?? throw new Exception("Stream null");
        }

        throw new Exception("Embedded resource not found: " + file_path);
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


///* 
// * © 2024 Tobias Sachs
// * GameAssetManager
// * 11.07.2024 
//*/

//using FontStashSharp;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using sachssoft.Sasogine.Graphics;

//namespace sachssoft.Sasogine;

//public class GameAssetManager
//{
//    private IMyGameApp _app;
//    private Dictionary<string, IAsset> _textures;
//    private Dictionary<string, IAsset> _models;
//    private Dictionary<string, IAsset> _shaders;
//    private Dictionary<string, IAsset> _fonts;
//    private Dictionary<string, IAsset> _data;

//    public GameAssetManager(IMyGameApp app)
//    {
//        _app = app;
//        _textures = new();
//        _models = new();
//        _shaders = new();
//        _fonts = new();
//        _data = new();
//    }

//    public IMyGameApp App => _app;

//    internal protected virtual void OnLoad()
//    {
//    }

//    internal protected virtual void OnUnload()
//    {
//    }    

//    public bool AddModel(string key, string file)
//    {
//        return _models.TryAdd(key, new SimpleAsset(file));
//    }

//    // Sonstige Daten mit den Dateinamen
//    public bool AddData(string key, string file)
//    {
//        //CheckIfExists(_data, key);
//        return _data.TryAdd(key, new SimpleAsset(file));
//    }

//    // Globaler Textur mit den Dateinamen
//    public bool AddTexture(string key, string file)
//    {
//        //CheckIfExists(_textures, key);
//        return _textures.TryAdd(key, new SimpleAsset(file));
//    }

//    // Regionabhängiger Textur mit den Dateinamen inklusive Präfix
//    public bool AddRegionTexture(string key, string file)
//    {
//        //CheckIfExists(_textures, key);
//        return _textures.TryAdd(key, new RegionDependentAsset(_app, file));
//    }

//    public Texture2D LoadTextureContent(string key)
//    {
//        CheckIfNotExists(_textures, key);
//        var asset = _textures[key];
//        return _app.Content.Load<Texture2D>(asset.GetFiles()[0]);
//        //return (Texture2D)(asset.Data ??= _app.Content.Load<Texture2D>(asset.GetFiles()[0]));
//    }

//    public Model LoadModelContent(string key)
//    {
//        CheckIfNotExists(_models, key);
//        var asset = _models[key];
//        return (Model)(asset.Data ??= _app.Content.Load<Model>(asset.GetFiles()[0]));
//    }

//    public void UnloadTextureContent(string key)
//    {
//        var asset = _textures[key];
//        _app.Content.UnloadAsset(asset.GetFiles()[0]);
//    }

//    public void UnloadModelContent(string key)
//    {
//        var asset = _models[key];
//        _app.Content.UnloadAsset(asset.GetFiles()[0]);
//    }

//    public Texture2D LoadTextureExternally(string key, string path)
//    {
//        CheckIfNotExists(_textures, key);

//        var asset = _textures[key];
//        if (asset.Data == null)
//        {
//            var file_path = path + System.IO.Path.DirectorySeparatorChar + asset.GetFiles()[0];
//            asset.Data = Texture2D.FromFile(_app.GraphicsDevice, file_path);
//        }

//        return (Texture2D)asset.Data;
//    }

//    public Texture2D LoadTextureResource(string key)
//    {
//        CheckIfNotExists(_textures, key);
//        var asset = _textures[key];
//        return (Texture2D)(asset.Data ??= Texture2D.FromStream(_app.GraphicsDevice, GetResourceStream(asset.GetFiles()[0])));
//    }



//    public System.IO.Stream LoadDataResource(string key)
//    {
//        CheckIfNotExists(_data, key);
//        var asset = _data[key];
//        return (System.IO.Stream)(asset.Data ??= GetResourceStream(asset.GetFiles()[0]));
//    }

//    // Shader mit den Dateinamen
//    public bool AddShader(string key, string file)
//    {
//        //CheckIfExists(_textures, key);
//        return _shaders.TryAdd(key, new SimpleAsset(file));
//    }

//    public Effect LoadShaderContent(string key)
//    {
//        CheckIfNotExists(_shaders, key);
//        var asset = _shaders[key];
//        return (Effect)(asset.Data ??= _app.Content.Load<Effect>(asset.GetFiles()[0]));
//    }

//    public bool AddFontSystem(string key, params string[] font_paths)
//    {
//        //CheckIfExists(_fonts, key);
//        return _fonts.TryAdd(key, new SimpleAsset(font_paths));
//    }

//    public FontSystem LoadFontSystemExternally(string key)
//    {
//        CheckIfNotExists(_fonts, key);
//        var asset = _fonts[key];

//        if (asset.Data == null)
//        {
//            var font_system = new FontSystem();
//            foreach (var path in asset.GetFiles())
//            {
//                font_system.AddFont(System.IO.File.ReadAllBytes(path));
//            }
//            GC.Collect();
//            asset.Data = font_system;
//        }

//        return (FontSystem)asset.Data;
//    }

//    private System.IO.Stream GetResourceStream(string file_path)
//    {
//        var name = (_app.Content.RootDirectory + "/" + file_path).Replace("/", ".").ToLower();
//        var assembly = _app.GetType().Assembly;

//        var names = assembly.GetManifestResourceNames();
//        System.IO.Stream? stream = null;

//        foreach (var item in names)
//        {
//            if (item.ToLower().EndsWith(name))
//            {
//                stream = assembly.GetManifestResourceStream(item);
//                break;
//            }
//        }

//        return stream ?? throw new GameException("Resource path not found");
//    }

//    private void CheckIfNotExists(IDictionary dict, string key)
//    {
//        if (!dict.Contains(key))
//            throw new GameException("Key not found");
//    }

//    //private void CheckIfExists(IDictionary dict, string key)
//    //{
//    //    if (dict.Contains(key))
//    //        throw new GameException("Key already exists");
//    //}

//    private interface IAsset
//    {
//        string[] GetFiles();

//        object? Data { get; set; }
//    }

//    // Einfacher Dateiname
//    private class SimpleAsset : IAsset
//    {
//        private string[] _files;

//        public SimpleAsset(params string[] file) => _files = file;

//        public string[] GetFiles()
//        {
//            return _files;
//        }

//        public object? Data { get; set; }
//    }

//    // Regionsabhängiger Dateiname
//    private class RegionDependentAsset : IAsset
//    {
//        private IMyGameApp _app;
//        private Dictionary<string, string> _files;

//        public RegionDependentAsset(IMyGameApp app, string file)
//        {
//            _app = app;
//            _files = new();

//            foreach (var region in _app.Regions)
//            {
//                _files.Add(region.Prefix, file + "_" + region.Prefix);
//            }
//        }

//        public string[] GetFiles()
//        {
//            return [_files[_app.CurrentRegion.Prefix]];
//        }

//        public object? Data { get; set; }
//    }

//    // Plattformabhängiger Dateiname
//    private class PlatformDependentAsset : IAsset
//    {
//        private IMyGameApp _app;
//        private Dictionary<PlatformProfiles, string> _files;

//        public PlatformDependentAsset(IMyGameApp app, string file)
//        {
//            _app = app;
//            _files = new();

//            switch (app.PlatformProfile)
//            {
//                case PlatformProfiles.OpenGL:
//                    // Ohne Präfix
//                    break;
//                case PlatformProfiles.DirectX:
//                    file += "_dx";
//                    break;
//                default:
//                    throw new NotSupportedException("Other profiles are not supported yet");
//            }

//            _files.Add(app.PlatformProfile, file);
//        }

//        public string[] GetFiles()
//        {
//            return [_files[_app.PlatformProfile]];
//        }

//        public object? Data { get; set; }
//    }
//}
