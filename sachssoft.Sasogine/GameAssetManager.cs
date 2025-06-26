/* 
 * © 2024 Tobias Sachs
 * GameAssetManager
 * 11.07.2024 
*/

using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using sachssoft.Sasogine.Graphics;

namespace sachssoft.Sasogine;

public class GameAssetManager
{
    private IMyGameApp _app;
    private Dictionary<string, IAsset> _textures;
    private Dictionary<string, IAsset> _models;
    private Dictionary<string, IAsset> _shaders;
    private Dictionary<string, IAsset> _fonts;
    private Dictionary<string, IAsset> _data;

    public GameAssetManager(IMyGameApp app)
    {
        _app = app;
        _textures = new();
        _models = new();
        _shaders = new();
        _fonts = new();
        _data = new();
    }

    public IMyGameApp App => _app;

    internal protected virtual void OnLoad()
    {
    }

    internal protected virtual void OnUnload()
    {
    }    
    
    public bool AddModel(string key, string file)
    {
        return _models.TryAdd(key, new SimpleAsset(file));
    }

    // Sonstige Daten mit den Dateinamen
    public bool AddData(string key, string file)
    {
        //CheckIfExists(_data, key);
        return _data.TryAdd(key, new SimpleAsset(file));
    }

    // Globaler Textur mit den Dateinamen
    public bool AddTexture(string key, string file)
    {
        //CheckIfExists(_textures, key);
        return _textures.TryAdd(key, new SimpleAsset(file));
    }

    // Regionabhängiger Textur mit den Dateinamen inklusive Präfix
    public bool AddRegionTexture(string key, string file)
    {
        //CheckIfExists(_textures, key);
        return _textures.TryAdd(key, new RegionDependentAsset(_app, file));
    }

    public Texture2D LoadTextureContent(string key)
    {
        CheckIfNotExists(_textures, key);
        var asset = _textures[key];
        return _app.Content.Load<Texture2D>(asset.GetFiles()[0]);
        //return (Texture2D)(asset.Data ??= _app.Content.Load<Texture2D>(asset.GetFiles()[0]));
    }

    public Model LoadModelContent(string key)
    {
        CheckIfNotExists(_models, key);
        var asset = _models[key];
        return (Model)(asset.Data ??= _app.Content.Load<Model>(asset.GetFiles()[0]));
    }

    public void UnloadTextureContent(string key)
    {
        var asset = _textures[key];
        _app.Content.UnloadAsset(asset.GetFiles()[0]);
    }

    public void UnloadModelContent(string key)
    {
        var asset = _models[key];
        _app.Content.UnloadAsset(asset.GetFiles()[0]);
    }

    public Texture2D LoadTextureExternally(string key, string path)
    {
        CheckIfNotExists(_textures, key);

        var asset = _textures[key];
        if (asset.Data == null)
        {
            var file_path = path + System.IO.Path.DirectorySeparatorChar + asset.GetFiles()[0];
            asset.Data = Texture2D.FromFile(_app.GraphicsDevice, file_path);
        }

        return (Texture2D)asset.Data;
    }

    public Texture2D LoadTextureResource(string key)
    {
        CheckIfNotExists(_textures, key);
        var asset = _textures[key];
        return (Texture2D)(asset.Data ??= Texture2D.FromStream(_app.GraphicsDevice, GetResourceStream(asset.GetFiles()[0])));
    }

    public System.IO.Stream LoadDataResource(string key)
    {
        CheckIfNotExists(_data, key);
        var asset = _data[key];
        return (System.IO.Stream)(asset.Data ??= GetResourceStream(asset.GetFiles()[0]));
    }

    // Shader mit den Dateinamen
    public bool AddShader(string key, string file)
    {
        //CheckIfExists(_textures, key);
        return _shaders.TryAdd(key, new SimpleAsset(file));
    }

    public Effect LoadShaderContent(string key)
    {
        CheckIfNotExists(_shaders, key);
        var asset = _shaders[key];
        return (Effect)(asset.Data ??= _app.Content.Load<Effect>(asset.GetFiles()[0]));
    }

    public bool AddFontSystem(string key, params string[] font_paths)
    {
        //CheckIfExists(_fonts, key);
        return _fonts.TryAdd(key, new SimpleAsset(font_paths));
    }

    public FontSystem LoadFontSystemExternally(string key)
    {
        CheckIfNotExists(_fonts, key);
        var asset = _fonts[key];

        if (asset.Data == null)
        {
            var font_system = new FontSystem();
            foreach (var path in asset.GetFiles())
            {
                font_system.AddFont(System.IO.File.ReadAllBytes(path));
            }
            GC.Collect();
            asset.Data = font_system;
        }

        return (FontSystem)asset.Data;
    }

    private System.IO.Stream GetResourceStream(string file_path)
    {
        var name = (_app.Content.RootDirectory + "/" + file_path).Replace("/", ".").ToLower();
        var assembly = _app.GetType().Assembly;

        var names = assembly.GetManifestResourceNames();
        System.IO.Stream? stream = null;

        foreach (var item in names)
        {
            if (item.ToLower().EndsWith(name))
            {
                stream = assembly.GetManifestResourceStream(item);
                break;
            }
        }

        return stream ?? throw new GameException("Resource path not found");
    }

    private void CheckIfNotExists(IDictionary dict, string key)
    {
        if (!dict.Contains(key))
            throw new GameException("Key not found");
    }

    //private void CheckIfExists(IDictionary dict, string key)
    //{
    //    if (dict.Contains(key))
    //        throw new GameException("Key already exists");
    //}

    private interface IAsset
    {
        string[] GetFiles();

        object? Data { get; set; }
    }

    // Einfacher Dateiname
    private class SimpleAsset : IAsset
    {
        private string[] _files;

        public SimpleAsset(params string[] file) => _files = file;

        public string[] GetFiles()
        {
            return _files;
        }

        public object? Data { get; set; }
    }

    // Regionsabhängiger Dateiname
    private class RegionDependentAsset : IAsset
    {
        private IMyGameApp _app;
        private Dictionary<string, string> _files;

        public RegionDependentAsset(IMyGameApp app, string file)
        {
            _app = app;
            _files = new();

            foreach (var region in _app.Regions)
            {
                _files.Add(region.Prefix, file + "_" + region.Prefix);
            }
        }

        public string[] GetFiles()
        {
            return [_files[_app.CurrentRegion.Prefix]];
        }

        public object? Data { get; set; }
    }

    // Plattformabhängiger Dateiname
    private class PlatformDependentAsset : IAsset
    {
        private IMyGameApp _app;
        private Dictionary<PlatformProfiles, string> _files;

        public PlatformDependentAsset(IMyGameApp app, string file)
        {
            _app = app;
            _files = new();

            switch (app.PlatformProfile)
            {
                case PlatformProfiles.OpenGL:
                    // Ohne Präfix
                    break;
                case PlatformProfiles.DirectX:
                    file += "_dx";
                    break;
                default:
                    throw new NotSupportedException("Other profiles are not supported yet");
            }

            _files.Add(app.PlatformProfile, file);
        }

        public string[] GetFiles()
        {
            return [_files[_app.PlatformProfile]];
        }

        public object? Data { get; set; }
    }
}
