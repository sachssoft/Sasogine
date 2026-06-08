namespace Sachssoft.Sasogine.Embedding
{

    // Statt GetBackBufferData performanfreundlichste GPU -> GPU mit Texture Sharing
    // Monogame RenderTarget2D -> andere Platform z.B. Avalonia
    public interface ISharedTextureProvider
    {

        SharedTextureContext? SharedTextureContext { get; }

    }
}
