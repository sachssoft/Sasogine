using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Resources
{
    public partial class GameResourceManager
    {
        static GameResourceManager()
        {
            RegisterLoader<EmbeddedResourceLoader>((res, path) => new EmbeddedResourceLoader(Path.Combine(res.RootDirectory, path), res.GameApplication.Assembly));
            RegisterLoader<LocalFileLoader>((res, path) => new LocalFileLoader(Path.Combine(res.RootDirectory, path)));
            
            // ------------------- Stream -------------------
            RegisterType<Stream>(
                (res, loader) => loader.GetStream(),
                (res, loader) => loader.GetStreamAsync()
            );

            // ------------------- Texture2D -------------------
            RegisterType<Texture2D>(
                (res, loader) =>
                {
                    if (res.GraphicsDevice == null) throw new InvalidOperationException("GraphicsDevice is not set in LoaderOptions.");
                    return Texture2D.FromStream(res.GraphicsDevice, loader.GetStream());
                },
                async (res, loader) =>
                {
                    if (res.GraphicsDevice == null) throw new InvalidOperationException("GraphicsDevice is not set in LoaderOptions.");
                    using var stream = await loader.GetStreamAsync();
                    return Texture2D.FromStream(res.GraphicsDevice, stream);
                }
            );

            // ------------------- Effect -------------------
            RegisterType<Effect>(
                (res, loader) =>
                {
                    using var stream = loader.GetStream();
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return new Effect(res.GraphicsDevice, ms.ToArray());
                },
                async (res, loader) =>
                {
                    using var stream = await loader.GetStreamAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    ms.Position = 0;
                    return new Effect(res.GraphicsDevice, ms.ToArray());
                }
            );

            // ------------------- XDocument -------------------
            RegisterType<XDocument>(
                (res, loader) =>
                {
                    using var stream = loader.GetStream();
                    return XDocument.Load(stream);
                },
                async (res, loader) =>
                {
                    using var stream = await loader.GetStreamAsync();
                    return await Task.Run(() => XDocument.Load(stream));
                }
            );

            // ------------------- JsonDocument -------------------
            RegisterType<JsonDocument>(
                (res, loader) =>
                {
                    using var stream = loader.GetStream();
                    return JsonDocument.Parse(stream);
                },
                async (res, loader) =>
                {
                    using var stream = await loader.GetStreamAsync();
                    return await Task.Run(() => JsonDocument.Parse(stream));
                }
            );

            //// ------------------- Model -------------------
            //RegisterType<Model>(
            //    (res, loader) =>
            //    {
            //        if (res.Content == null) throw new InvalidOperationException("ContentManager is null");
            //        return app.Content.Load<Model>(loader.FilePath); // FilePath ohne .xnb
            //    },
            //    async (res, loader) =>
            //    {
            //        // ContentManager ist synchron, Task.FromResult reicht
            //        if (res.Content == null) throw new InvalidOperationException("ContentManager is null");
            //        return await Task.FromResult(res.Content.Load<Model>(loader.FilePath));
            //    }
            //);

            // ------------------- SoundEffect -------------------
            RegisterType<SoundEffect>(
                (res, loader) =>
                {
                    using var stream = loader.GetStream();
                    return SoundEffect.FromStream(stream);
                },
                async (res, loader) =>
                {
                    using var stream = await loader.GetStreamAsync();
                    return SoundEffect.FromStream(stream);
                }
            );
            
            // ------------------- FontStashSharp FontSystem -------------------
            RegisterType<FontSystem>(
                (res, loader) =>
                {
                    using var stream = loader.GetStream();
                    var fontSystem = new FontSystem();
                    fontSystem.AddFont(stream);
                    return fontSystem;
                },
                async (res, loader) =>
                {
                    using var stream = await loader.GetStreamAsync();
                    var fontSystem = new FontSystem();
                    fontSystem.AddFont(stream);
                    return fontSystem;
                }
            );
        }
    }
}
